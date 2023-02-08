using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace vp.models
{
    public abstract class BaseModel
    {
        protected BaseModel() { }
        protected BaseModel(string id)
        {
            _id = id;
        }

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string _id { get; set; } = null;
    }
}
