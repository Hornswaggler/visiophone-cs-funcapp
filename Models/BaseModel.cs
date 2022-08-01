using MongoDB.Bson;

namespace vp.models
{
    public abstract class BaseModel
    {
        public ObjectId _id { get; set; }
        protected bool _paginated { get; set; } = false;
        public int page { get; set; }
    }
}
