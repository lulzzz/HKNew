namespace HK_Project.ViewModels
{
    public class lanViewModel
    {
        public string language { get; set; }
        public double score { get; set; }
        public bool isTranslationSupported { get; set; }
        public bool isTransliterationSupported { get; set; }
    }

    public class TranslateViewModel
    {
        public string text { get; set; }
        public string to { get; set; }
    }
    public class TranslationsViewModel
    {
        public List<TranslateViewModel> translations { get; set; }
    }
}
