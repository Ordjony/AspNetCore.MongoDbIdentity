﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AspNetCore.MongoDbIdentity
{
    public class UserStore<TUser> : IUserStore<TUser>,
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>
        where TUser : MongoIdentityUser
    {
        private readonly IMongoCollection<TUser> _userCollection;

        public UserStore(IMongoCollection<TUser> userCollection)
        {
            _userCollection = userCollection;
        }

        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            user.Id = ObjectId.GenerateNewId().ToString();
            await _userCollection.InsertOneAsync(user, new InsertOneOptions(), cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user, new UpdateOptions(), cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            await _userCollection.DeleteOneAsync(u => u.Id == user.Id, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (!IsObjectId(userId))
            {
                return null;
            }

            return await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _userCollection.Find(u => u.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync(cancellationToken);
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (user.Logins.Any(l => l.Equals(login)))
            {
                throw new InvalidOperationException($"Login {login.LoginProvider} is already exists.");
            }

            user.AddLogin(login);
            return Task.CompletedTask;
        }

        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.RemoveLogin(loginProvider, providerKey);
            return Task.CompletedTask;
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var logins = user.Logins
                .Select(l => l.ToUserLoginInfo())
                .ToList();

            return Task.FromResult<IList<UserLoginInfo>>(logins);
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return await _userCollection.Find(u => u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var claims = user.Claims.Select(c => c.ToSecurityClaim()).ToList();
            return Task.FromResult<IList<Claim>>(claims);
        }

        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            foreach (var claim in claims)
            {
                user.AddClaim(claim);
            }

            return Task.CompletedTask;
        }

        public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            user.ReplaceClaim(claim, newClaim);
            return Task.CompletedTask;
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            foreach (var claim in claims)
            {
                user.RemoveClaim(claim);
            }

            return Task.CompletedTask;
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            return await _userCollection.Find(u => u.Claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                .ToListAsync(cancellationToken);
        }

        protected bool IsObjectId(string id)
        {
            return ObjectId.TryParse(id, out var objId);
        }
    }
}