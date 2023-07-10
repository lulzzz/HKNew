using HKDB.Data;
using HKDB.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HK_Project.Interface;
using HK_Project.Services;
using HK_Project.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HK_Project.Controllers
{
    public class LoginRegisterController : Controller
    {
        private readonly HKContext _ctx;
        private readonly IHashService _hashService;
        private readonly AccountServices _accountServices;
        private readonly ClaimServer _claimServer;

        public LoginRegisterController(HKContext ctx, AccountServices accountServices, IHashService hashService, ClaimServer claimServer)
        {
            _ctx = ctx;
            _accountServices = accountServices;
            _hashService = hashService;
            _claimServer = claimServer;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(UserInfoViewModel lvm)
        {
            if (ModelState.IsValid)
            {

                var member = await _accountServices.AuthenticateMember(lvm);

                if (member == null)
                {
                    ModelState.AddModelError(string.Empty, "帳號密碼有錯!!!");
                    return View(lvm);
                }
                _claimServer.ClaimAdd(lvm.Email);

                return RedirectToAction("Index", "Member");

            }

            return View(lvm);
        }
        [HttpGet]
        public IActionResult SignUp()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(SingupViewModel member)
        {
            if (ModelState.IsValid)
            {

                var Samememberemail = await _ctx.Members.SingleOrDefaultAsync(u => u.MemberEmail == member.Email);

                if (Samememberemail != null)
                {
                    ViewBag.ErrorMessage = "SignUp failed: email already exists.";
                    return View(member);
                }
                else
                {
                    //會員資料寫入DB
                    member.Password = _hashService.MD5Hash(member.Password);
                    
                    Member m = new Member()
                    {
                        MemberEmail = member.Email,
                        MemberName = "Member",
                        MemberPassword = member.Password
                    };

                    _ctx.Add(m);
                    await _ctx.SaveChangesAsync();
                    //cookie 帶電子郵件
                    _claimServer.ClaimAdd(member.Email);

                    return RedirectToAction("Index", "Member");
                }
            }
            return View();
        }

        //登出
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
