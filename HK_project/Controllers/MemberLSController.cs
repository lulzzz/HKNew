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
    public class MemberLSController : Controller
    {
        private readonly HKContext _ctx;
        private readonly IHashService _hashService;
        private readonly AccountService _accountServices;
        private readonly ClaimService _claimServer;
        private readonly LINQService _lq;


        public MemberLSController(HKContext ctx, AccountService accountServices, IHashService hashService, ClaimService claimServer, LINQService linqService)
        {
            _ctx = ctx;
            _accountServices = accountServices;
            _hashService = hashService;
            _claimServer = claimServer;
            _lq = linqService;

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
        public async Task<IActionResult> LogIn(PasswordLSViewModel LoginVM)
        {
            if (ModelState.IsValid)
            {

                var member = await _accountServices.AuthenticateMember(LoginVM);

                if (member == null)
                {
                    ModelState.AddModelError(string.Empty, "帳號密碼有錯!!!");
                    return View(LoginVM);
                }
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(await _claimServer.ClaimAdd(member.Email)));

                return RedirectToAction("MemberIndex", "Chat");

            }

            return View(LoginVM);
        }
        [HttpGet]
        public IActionResult SignUp()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(PasswordLSViewModel member)
        {
            if (ModelState.IsValid)
            {

                var Samememberemail = await _lq.GetMember(member.Email);

                if (Samememberemail != null)
                {
                    ViewBag.ErrorMessage = "SignUp failed: email already exists.";
                    return View(member);
                }
                else
                {
                    //會員資料寫入DB
                    member.Password = _hashService.MD5Hash(member.Password);
                    
                    Member m = new()
                    {
                        MemberEmail = member.Email,
                        MemberName = "Member",
                        MemberPassword = member.Password
                    };

                    _ctx.Add(m);

                    User u = new User()
                    {
                        UserName = "User",
                        UserEmail = member.Email,
                        UserPassword = member.Password
                    };
                    _ctx.Add(u);

                    await _ctx.SaveChangesAsync();
                    //cookie 帶電子郵件
                    await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(await _claimServer.ClaimAdd(member.Email)));

                    return RedirectToAction("MemberIndex", "Chat");
                }
            }
            return View();
        }

        //登出
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Show", "Home");
        }
    }
}
