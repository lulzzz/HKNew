using HK_Project.Interface;
using HK_Project.Services;
using HKDB.Data;
using HKDB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HK_Project.Controllers
{
    public class ChatController : Controller
    {
        private readonly HKContext _ctx;
        private readonly IHashService _hashService;
        private readonly AccountService _accountServices;
        private readonly ILogger<MemberController> _logger;

        public ChatController(HKContext ctx, AccountService accountServices, IHashService hashService, ILogger<MemberController> logger)
        {
            _ctx = ctx;
            _accountServices = accountServices;
            _hashService = hashService;
            _logger = logger;
        }
        public IActionResult UserIndex()
        {
            return View();
        }
        public IActionResult MemberIndex()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChooseApp()
        {
            var applications = _ctx.Applications.Where(a => a.ApplicationId != null).ToList();

            ViewBag.Applist = applications;
            return View();

        }
        [HttpPost]
        public IActionResult ChooseApp(string Email)
        {
            var AppSearch = from a in _ctx.Applications
                            join m in _ctx.Members on a.MemberId equals m.MemberId
                            where m.MemberEmail == Email
                            select a;

            ViewBag.AppSearch = AppSearch;           

            return RedirectToAction("Qa", "User");
        }

        [HttpPost]
        public async Task<IActionResult> Qa ()
        {
            var Email = User.FindFirstValue(ClaimTypes.Email);
            
            var UserList = _ctx.Users.FirstOrDefault(u => u.UserEmail == Email);
            
            var ChatSearch = from c in _ctx.Chats
                             join u in _ctx.Users on c.UserId equals u.UserId
                             where u.UserEmail == Email
                             select c;
            
            if (ChatSearch == null)
            {
                Chat Chat = new Chat()
                {
                    ChatTime = DateTime.Now,
                    ChatName = "Chat_" + UserList.UserId.ToString().PadLeft(4, '0'),
                    UserId = UserList.UserId
                };

                _ctx.Chats.Add(Chat);
                await _ctx.SaveChangesAsync();
                
                ViewBag.Chat = Chat;
                return View(ViewBag);
            }
            else
            {
                ViewBag.Chat = ChatSearch;
                return View(ViewBag);
            }
        }


        public IActionResult ChatHistory(int chatid)
        {
            var chatHistory = _ctx.QAHistorys.Where(c => c.ChatId == chatid).ToList();
            return View(chatHistory);
        }

        [HttpPost]
        public IActionResult GetChatHistory(int id)
        {
            var chatHistory = _ctx.QAHistorys.Where(q => q.ChatId == id).ToList();
            TempData["Userchatid"] = id;
            return Json(chatHistory);
        }
    }
}
