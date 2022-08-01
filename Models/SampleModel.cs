namespace vp.models {
	public class SampleRequest : BaseModel
	{
		public SampleRequest() : base() {
			this._paginated = true;
		}

		public string tag { get; set; }

		public string description { get; set; }
	}

}
