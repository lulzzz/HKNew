namespace OneChatPage.ViewModels
{
    public class EmbViewModel
    {
		public List<string>? Refer { get; set; }

		public string? id { get; set; }
		public string Message { get; set; }
		public float[] MessageEmbedding { get; set; }
		public string? Anser { get; set; }
	}

}
