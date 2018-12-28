using MongoDB.Bson.Serialization.Attributes;

namespace AspNetCore.MongoDbIdentity
{
    public class MongoIdentityRole
    {
        [BsonId]
        public string Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }
}
