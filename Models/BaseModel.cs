namespace vp.models
{
    public abstract class BaseModel
    {
        protected BaseModel() { }
        protected BaseModel(string id)
        {
            id = id;
            _id = id;
        }

        public string _id { get; set; } = null;
        public string id { get; set; } = null;
    }
}
