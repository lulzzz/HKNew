using Microsoft.AspNetCore.Mvc;

namespace HK_project.Controllers
{
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
