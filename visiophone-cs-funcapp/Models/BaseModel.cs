namespace vp.models
{
    public abstract class BaseModel
    {
        protected BaseModel() { }
        protected BaseModel(string id)
        {
            this.id = id;
        }

        public string id { get; set; } = null;
    }
}
