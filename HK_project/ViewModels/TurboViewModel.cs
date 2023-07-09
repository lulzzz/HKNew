using HKDB.Models;

namespace HK_project.ViewModels
{
    public class TurboViewModel
    {
        public string DataId { get; set; }
        public string Question { get; set; }
        public string Sim_Anser { get; set; }
        public Application Setting { get; set; }
        public float temperature { get; set; }
        public string ChatId { get; set; }
    }
}
