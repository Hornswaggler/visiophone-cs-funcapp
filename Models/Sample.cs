using MongoDB.Bson;

namespace vp.Models {
	public class SampleModel
	{
		public SampleModel() { }

		public ObjectId _id { get; set; }

		//public string ownerId { get; set; }

		public string tag { get; set; }

		public string description { get; set; }

	}

}
