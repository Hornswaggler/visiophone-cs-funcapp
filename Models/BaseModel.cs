namespace vp.models
{
    public abstract class BaseModel
    {
        protected BaseModel() { }
        protected BaseModel(string id)
        {
            id = id;
        }

        public string id { get; set; } = null;
    }
}
