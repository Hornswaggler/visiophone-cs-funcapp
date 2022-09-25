namespace vp.models {
	public class SampleModel : BaseModel
	{
		public SampleModel() : base() {}

		public string tag { get; set; } = "";

		public string description { get; set; } = "";

		public string seller { get; set; } = "";

		public string bpm { get; set; } = "";

		public int cost { get; set; } = 0;
	}
}
