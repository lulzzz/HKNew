using Microsoft.AspNetCore.Mvc;
using HK_Project.ViewModels;
using HKDB.Models;
using HKDB.Data;
using System.Security.Claims;
using HK_Project.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace HK_Project.Controllers
{
    public class UserLSController : Controller
    {
        private readonly HKContext _ctx;
        private readonly ClaimService _claim;
        private readonly LINQService _lq;
        public UserLSController(HKContext ctx, ClaimService claim, LINQService lq)
        {
            _ctx = ctx;
            _claim = claim;
            _lq = lq;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> UserLoginSingup(EmailLSViewModel lvm)
        {
            if (ModelState.IsValid)
            {
                var UserEmail_exist = _lq.GetUser(lvm.Email);

                if(UserEmail_exist == null)
                {
                    
                    User user = new User()
                    {
                        UserEmail = lvm.Email
                    };

                    _ctx.Users.Add(user);
                    await _ctx.SaveChangesAsync();
                }

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(await _claim.ClaimAdd(lvm.Email)));
                
                return RedirectToAction("UserIndex", "Chat");
            }
            return View(lvm);
        }





    }
}
