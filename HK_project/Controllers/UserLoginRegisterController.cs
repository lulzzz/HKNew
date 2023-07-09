using Microsoft.AspNetCore.Mvc;

namespace HK_project.Controllers
{
    public class UserLoginRegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
