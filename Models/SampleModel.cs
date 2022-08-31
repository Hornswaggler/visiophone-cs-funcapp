namespace vp.models {
	public class SampleModel : BaseModel
	{
		public SampleModel() : base() {}

		public string tag { get; set; } = "";

		public string description { get; set; } = "";

		public string fileId { get; set; } = "";
	}
}
