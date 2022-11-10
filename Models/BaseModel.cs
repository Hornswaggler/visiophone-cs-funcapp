using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace vp.models
{
    public abstract class BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string _id { get; set; } = null;
    }
}
