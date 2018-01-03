using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AspNetCore.MongoDbIdentity
{
    public class UserStore<TUser> : IUserStore<TUser>
        where TUser : IdentityUser
    {
        private readonly IMongoCollection<TUser> _mongoCollection;

        public UserStore(IMongoCollection<TUser> mongoCollection)
        {
            _mongoCollection = mongoCollection;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
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
            await _mongoCollection.InsertOneAsync(user, new InsertOneOptions(), cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            await _mongoCollection.ReplaceOneAsync(u => u.Id == user.Id, user, new UpdateOptions(), cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            await _mongoCollection.DeleteOneAsync(u => u.Id == user.Id, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (!IsObjectId(userId))
            {
                return null;
            }

            return await _mongoCollection.Find(u => u.Id == userId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            if (!IsObjectId(normalizedUserName))
            {
                return null;
            }

            return await _mongoCollection.Find(u => u.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync(cancellationToken);
        }

        protected bool IsObjectId(string id)
        {
            return ObjectId.TryParse(id, out var objId);
        }
    }
}