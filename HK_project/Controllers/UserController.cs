using Microsoft.AspNetCore.Mvc;

namespace HK_project.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
