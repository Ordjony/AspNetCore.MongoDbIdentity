namespace AspNetCore.MongoDbIdentity
{
    public class IdentityUser
    {
        public virtual string Id { get; set; }

        public virtual string UserName { get; set; }

        public virtual string NormalizedUserName { get; set; }
    }
}
