using HK_Project.Interface;
using HK_Project.Services;
using HK_Project.ViewModels;
using HKDB.Data;
using HKDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult ChooseApp(string ApplicationId, string Parameter, string ApplicationName)//不會回傳使用者email  
        {
            //var applications = _ctx.Applications.Where(a => a.ApplicationId != null).ToList();
            //ViewBag.Applist = applications;
            TempData["ApplicationId"] = ApplicationId;
            TempData["Parameter"] = Parameter;
            TempData["ApplicationName"] = ApplicationName;

            return View();//RedirectToAction("Qa","Chat")
        }

        [HttpPost]
        public IActionResult EnterMemberEmail(string Email)
        {
            var AppSearch = from a in _ctx.Applications
                            join m in _ctx.Members on a.MemberId equals m.MemberId
                            where m.MemberEmail == Email
                            select a;

            ViewBag.AppSearch = AppSearch;
            return View(ViewBag);
        }

        [HttpGet]
        public async Task<IActionResult> Qa()
        {
            var Email = User.FindFirstValue(ClaimTypes.Email);
            var UserList = _ctx.Users.FirstOrDefault(u => u.UserEmail == Email);

            var ChatSearch = from c in _ctx.Chats
                             orderby c.ChatTime descending
                             join u in _ctx.Users on c.UserId equals u.UserId
                             where u.UserEmail == Email
                             select c;

            List<Chat> chatList = new List<Chat>();

            if (ChatSearch.Count() == 0)
            {
                Chat Chat = new Chat()
                {
                    ChatTime = DateTime.Now,
                    ChatName = "NewChat",
                    UserId = UserList.UserId
                };

                _ctx.Chats.Add(Chat);
                await _ctx.SaveChangesAsync();

                chatList.Add(Chat);
            }
            else
            {
                chatList = ChatSearch.ToList();
            }

            ViewBag.Chats = chatList;

            return View();
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


        [HttpPost]
        public async Task<IActionResult> qainput(string question)
        {

            //var appid = TempData["ApplicationId"].ToString();
            //var temp = TempData["Parameter"].ToString();
            //var chatid = TempData["Userchatid"].ToString();
            //var q = question;
            //var file = await _ctx.AIFiles.FirstOrDefaultAsync(c => c.ApplicationId == appid);
            //var fileid = file.AifileId;
            //fileid = "D0003";

            //var client = new HttpClient();
            //string jsonContent = $@"{{
            //                        ""ApplicationId"": ""{appid}"",
            //                        ""temperature"": ""{temp}"",
            //                        ""ChatId"": ""{chatid}"",
            //                        ""Question"": ""{q}"",
            //                        ""DataId"": ""{fileid}""
            //                    }}";

            //var content = new StringContent(jsonContent, null, "application/json");
            //var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7168/api/Similar");
            //request.Content = content;

            //var response = await client.SendAsync(request);

            //response.EnsureSuccessStatusCode();


            return Json(0);//response
        }



        [HttpPost]
        public async Task<IActionResult> Creatchat()
        {
            try
            {
                var useremail = User.FindFirstValue(ClaimTypes.Email);
                var userlist = await _ctx.Users.FirstOrDefaultAsync(u => u.UserEmail == useremail);
                
                Chat chat = new Chat()
                {
                    
                    ChatTime = DateTime.Now,
                    ChatName = "New Chat",
                    UserId = userlist.UserId
                };
                _ctx.Chats.Add(chat);
                await _ctx.SaveChangesAsync();
                return Json(chat);//chat
            }
            catch (Exception ex)
            {
                // Handle the error
                return Json(new { error = ex.Message });
            }
        }













    }
}
