using HKDB.Models;

namespace HK_Project.ViewModels;
public class QagptViewModel
{
    public Chat Chat { get; set; }
    public Qahistory QA { get; set; }
    public User User { get; set; }
}
