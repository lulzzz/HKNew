//using HK_Project.Interface;
//using HK_Project.Services;
//using HK_Project.ViewModels;
//using HKDB.Data;
//using HKDB.Models;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using Microsoft.EntityFrameworkCore;

//namespace HK_Project.Controllers
//{
//    public class UserLoginRegisterController : Controller
//    {

//        private readonly HKContext _ctx;
//        private readonly IHashService _hashService;
//        private readonly AccountServices _accountServices;

//        public UserLoginRegisterController(HKContext ctx, AccountServices accountServices, IHashService hashService)
//        {
//            _ctx = ctx;
//            _accountServices = accountServices;
//            _hashService = hashService;
//        }
//        public IActionResult Index()
//        {
//            return View();
//        }
//        [HttpGet]
//        public IActionResult SingUp()
//        {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> SingUp(SingupViewModel user)
//        {
//            if (ModelState.IsValid)
//            {

//                var UserEmailinMemberEmail = await _ctx.Members.SingleOrDefaultAsync(u => u.MemberEmail == user.UserEmail);

//                var UserEmailinUserEmail = await _ctx.Users.SingleOrDefaultAsync(u => u.UserEmail == user.UserEmail);
//                if (UserEmailinUserEmail == null)
//                {
//                    //var maxuserid = _ctx.Users.OrderByDescending(c => c.UserId).FirstOrDefault();
//                    //int newId = int.Parse(maxuserid.UserId.Substring(1)) + 1;
//                    //string newUserId = "U" + newId.ToString().PadLeft(4, '0');
//                    //新增user
//                    User user1 = new User()
//                    {
//                        UserName = "User",
//                        UserEmail = user.UserEmail,
//                        UserPassword = _hashService.MD5Hash(user.UserPassword),

//                    };
//                    _ctx.Users.Add(user1);
//                    await _ctx.SaveChangesAsync();

//                    //建立授權
//                    var claims = new List<Claim>
//                        {
//                            new Claim(ClaimTypes.NameIdentifier, user1.UserId)  ,
//                            new Claim(ClaimTypes.Email, user1.UserEmail)
//                        };

//                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

//                    await HttpContext.SignInAsync(
//                        CookieAuthenticationDefaults.AuthenticationScheme,
//                        new ClaimsPrincipal(claimsIdentity)
//                        );

//                    return RedirectToAction("Index", "User");
//                }
//                else
//                {
//                    //報錯
//                    ViewBag.ErrorMessage = "Signin failed: email already exists.";
//                    return View(user);
//                }
//            }
//            else
//            {
//                //報錯
//                ViewBag.ErrorMessage = "Signin failed: email already exists.";
//                return View(user);
//            }

//            return View(user);
//        }

//        [HttpGet]
//        public IActionResult LogIn()
//        {
//            return View();
//        }
//        //登入
//        [HttpPost]
//        public async Task<IActionResult> LogIn(LoginViewModel lvm)
//        {
//            if (ModelState.IsValid)
//            {

//                User user = await _accountServices.AuthenticateUser(lvm);

//                if (user == null)
//                {
//                    ModelState.AddModelError(string.Empty, "帳號密碼有錯!!!");
//                    return View(lvm);
//                }
//                //驗證帳號密碼

//                //建立授權
//                var claims = new List<Claim>
//                {
//                    new Claim(ClaimTypes.NameIdentifier, user.UserId)  ,
//                    new Claim(ClaimTypes.Email, user.UserEmail)
//                };

//                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

//                await HttpContext.SignInAsync(
//                    CookieAuthenticationDefaults.AuthenticationScheme,
//                    new ClaimsPrincipal(claimsIdentity)
//                    );

//                return RedirectToAction("Index", "User");

//            }

//            return View(lvm);
//        }
//    }
//}
