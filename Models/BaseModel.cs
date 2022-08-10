using MongoDB.Bson;

namespace vp.models
{
    public abstract class BaseModel
    {
        public ObjectId _id { get; set; }
    }
}
