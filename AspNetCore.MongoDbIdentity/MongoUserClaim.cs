using System;
using System.Security.Claims;

namespace AspNetCore.MongoDbIdentity
{
    public class MongoUserClaim : IEquatable<MongoUserClaim>, IEquatable<Claim>
    {
        public MongoUserClaim()
        {
        }

        public MongoUserClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            Type = claim.Type;
            Value = claim.Value;
        }

        public string Type { get; private set; }
        public string Value { get; private set; }


        public bool Equals(MongoUserClaim other)
        {
            return string.Equals(other?.Type, Type)
                   && string.Equals(other?.Value, Value);
        }

        public bool Equals(Claim other)
        {
            return string.Equals(other?.Type, Type)
                   && string.Equals(other?.Value, Value);
        }

        public Claim ToSecurityClaim()
        {
            return new Claim(Type, Value);
        }
    }
}
