namespace OneChatPage.ViewModels
{
	public class SimDatasViewModel
	{
		public List<SimData> SimDatas { get; set; }
	}
	public class SimData
	{
		public required float[] MessageEmbedding { get; set; }
		public required string DbEmbedding { get; set; }
		public required int DbId { get; set; }
		public float? Similarity { get; set; }
	}
}
