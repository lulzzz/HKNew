using HK_Project.Extensions;
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
        private readonly ILogger<MemberFunctionController> _logger;
        private readonly LINQService _lq;

        public ChatController(HKContext ctx, AccountService accountServices, IHashService hashService, ILogger<MemberFunctionController> logger, LINQService lq)
        {
            _ctx = ctx;
            _accountServices = accountServices;
            _hashService = hashService;
            _logger = logger;
            _lq = lq;
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
            var UserList = await _lq.GetUser(Email);

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
            //var chatid = TempData["chatid"].ToString();
            //var q = question;
            //var file = await _ctx.AiFiles.FirstOrDefaultAsync(c => c.ApplicationId == appid);
            //var fileid = file.AifileId;

            //var client = new HttpClient();
            //string jsonContent = $@"{{
            //                        ""ApplicationId"": ""{appid}"",
            //                        ""temperature"": ""{temp}"",
            //                        ""ChatId"": ""{chatid}"",
            //                        ""Question"": ""{q}""
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

                Chat chat1 = new Chat()
                {
                    ChatTime = DateTime.Now,
                    ChatName = "NewChat",
                    UserId = userlist.UserId
                };
                _ctx.Chats.Add(chat1);
                await _ctx.SaveChangesAsync();  

                // create a ChatDto object and copy properties from chat1
                ChatDto chatDto = new ChatDto()
                {
                    ChatId = chat1.ChatId,
                    ChatTime = chat1.ChatTime,
                    ChatName = chat1.ChatName,
                    UserId = chat1.UserId
                };
                TempData["chatid"] = chat1.ChatId;

                // return the DTO object instead of the entity
                return Json(chatDto);
            }
            catch (Exception ex)
            {
                // Handle the error
                return Json(new { error = ex.Message });
            }
        }


        public class ChatDto
        {
            public int ChatId { get; set; }
            public DateTime ChatTime { get; set; }
            public string? ChatName { get; set; }
            public int UserId { get; set; }
        }












    }
}
