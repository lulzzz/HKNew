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

        public LoginRegisterController(HKContext ctx, AccountServices accountServices, IHashService hashService)
        {
            _ctx = ctx;
            _accountServices = accountServices;
            _hashService = hashService;
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
        public async Task<IActionResult> LogIn(LoginViewModel lvm)
        {
            if (ModelState.IsValid)
            {

                var member = await _accountServices.AuthenticateMember(lvm);

                if (member == null)
                {
                    ModelState.AddModelError(string.Empty, "帳號密碼有錯!!!");
                    return View(lvm);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, member.Email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, 
                    CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity));

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
        public async Task<IActionResult> SignUp([Bind("MemberEmail,MemberPassword")] Member member)
        {
            if (ModelState.IsValid)
            {

                var Samememberemail = await _ctx.Members.SingleOrDefaultAsync(u => u.MemberEmail == member.MemberEmail);
                //var MemberWithMaxId = await _ctx.Members.OrderByDescending(u => u.MemberId).FirstOrDefaultAsync();
                //var appWithMaxId = await _ctx.Applications.OrderByDescending(u => u.ApplicationId).FirstOrDefaultAsync();

                if (Samememberemail != null)
                {
                    ViewBag.ErrorMessage = "SigiUp failed: email already exists.";
                    return View(member);
                }
                else
                {
                    //if (MemberWithMaxId.MemberId != null)
                    //{
                    //    int maxId = int.Parse(MemberWithMaxId.MemberId.Substring(1));

                    //    int newId = maxId + 1;

                    //    newMemberId = "C" + newId.ToString().PadLeft(4, '0');

                    //}
                    //else
                    //{
                    //    newMemberId = "C0001";
                    //}

                    //會員資料寫入DB
                    member.MemberPassword = _hashService.MD5Hash(member.MemberPassword);
                    
                    Member m = new Member()
                    {
                        MemberEmail = member.MemberEmail,
                        MemberName = "Member",
                        MemberPassword = member.MemberPassword
                    };

                    _ctx.Add(m);
                    await _ctx.SaveChangesAsync();
                    //cookie 帶電子郵件
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, member.MemberEmail)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity)
                        );

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
