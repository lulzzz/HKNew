//using HK_Project.Services;
//using HK_Project.Interface;
//using HKDB.Data;
//using HKDB.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace HK_Project.Controllers
//{
//    public class UserController : Controller
//    {
//        private readonly HKContext _ctx;
//        private readonly IHashService _hashService;
//        private readonly AccountServices _accountServices;
//        private readonly ILogger<MemberController> _logger;

//        public UserController(HKContext ctx, AccountServices accountServices, IHashService hashService, ILogger<MemberController> logger)
//        {
//            _ctx = ctx;
//            _accountServices = accountServices;
//            _hashService = hashService;
//            _logger = logger;
//        }
//        public IActionResult Index()
//        {
//            return View();
//        }
//        [HttpGet]
//        public async Task<IActionResult> Chooseapp()
//        {
//            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            var user = await _ctx.Users.FirstOrDefaultAsync(m => m.UserId.ToString() == userid);
//            var applicationslist = _ctx.Applications.Where(a => a.MemberId == user.UserId).ToList();

//            ViewBag.Applist = applicationslist;
//            return View();

//        }
//        [HttpPost]
//        public IActionResult Chooseapp(string applicationId, string applicationname, string parameter)
//        {
//            TempData["Userchooseappid"] = applicationId;
//            TempData["Userchoosename"] = applicationname;
//            TempData["Userchooseparameter"] = parameter;
//            //待

//            return RedirectToAction("Qa", "User");

//        }
//        public IActionResult Qa()
//        {
//            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            var chat = _ctx.Chats.Where(c => c.UserId.ToString() == userid).OrderByDescending(c => c.ChatTime);
//            var chatmaxid = chat.OrderByDescending(c => c.ChatId).FirstOrDefault();
//            if (chatmaxid == null)
//            {
//                var userids = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
//                //var chatid = _ctx.Chats.OrderByDescending(c => c.ChatId).FirstOrDefault();
//                //int newId = int.Parse(chatid.ChatId.Substring(1)) + 1;
//                //string newUserId = "C" + newId.ToString().PadLeft(4, '0');
//                var chatid = "C" + userid.ToString().PadLeft(4, '0');
//                Chat chat1 = new Chat()
//                {
//                    ChatTime = DateTime.Now,
//                    ChatName = chatid,
//                    UserId = userids,
//                };
//                var QahistoryFirst = _ctx.QAHistory.Where(q => q.ChatId == chat1.ChatId).ToList();
//                ViewBag.ChatQaHistoryfirst = QahistoryFirst;
//                TempData["Userchatid"] = chat1.ChatId;

//                if (TempData["Userchoosename"] != null)
//                {
//                    var choosename = TempData["Userchoosename"].ToString();
//                    HttpContext.Session.SetString("Userchoosename", choosename);
//                }
//                return View();
//            }
//            var Qahistory = _ctx.QAHistory.Where(q => q.ChatId == chatmaxid.ChatId).ToList();
//            var chatlist = chat.ToList();
//            ViewBag.Userchatlist = chatlist;
//            ViewBag.ChatQaHistory = Qahistory;

//            TempData["Userchatid"] = chatmaxid.ChatId;

//            if (TempData["Userchoosename"] != null)
//            {
//                var choosename = TempData["Userchoosename"].ToString();
//                HttpContext.Session.SetString("Userchoosename", choosename);
//            }
//            return View();
//        }


//        [HttpPost]
//        public async Task<IActionResult> Creatchat()
//        {
//            try
//            {
//                // your existing code here
//                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
//                var chatmaxid = _ctx.Chats.OrderByDescending(c => c.ChatId).FirstOrDefault();
//                int newId = int.Parse(chatmaxid.ChatId.Substring(1)) + 1;
//                string newchatId = "C" + newId.ToString().PadLeft(4, '0');
//                Chat chat = new Chat()
//                {
//                    ChatId = newchatId,
//                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
//                    ChatTime = DateTime.Now,
//                    ChatName = newchatId
//                };
//                _ctx.Chats.Add(chat);
//                await _ctx.SaveChangesAsync();
//                TempData["Userchatid"] = newchatId;
//                return Json(chat);
//            }
//            catch (Exception ex)
//            {
//                // Handle the error
//                return Json(new { error = ex.Message });
//            }
//        }


//        public IActionResult ChatHistory(string chatid)
//        {
//            var chatHistory = _ctx.QAHistory.Where(c => c.ChatId == chatid).ToList();
//            return View(chatHistory);
//        }

//        [HttpPost]
//        public IActionResult GetChatHistory(string id)
//        {
//            var chatHistory = _ctx.QAHistory.Where(q => q.ChatId == id).ToList();
//            TempData["Userchatid"] = id;
//            return Json(chatHistory);
//        }


//[HttpPost]
//public async Task<IActionResult> qainput(string question)
//{

//    var appid = TempData["Userchooseappid"].ToString();
//    appid = "A0001";
//    TempData["Userchooseparameter"] = 1;
//    var temp = TempData["Userchooseparameter"].ToString();
//    //  temp = "1024";
//    var chatid = TempData["Userchatid"].ToString();
//    chatid = "C0001";
//    var q = question;
//    var file = await _ctx.AIFiles.FirstOrDefaultAsync(c => c.ApplicationId == appid);
//    var fileid = file.AifileId;
//    fileid = "D0003";

//    var client = new HttpClient();
//    string jsonContent = $@"{{
//                                    ""ApplicationId"": ""{appid}"",
//                                    ""temperature"": ""{temp}"",
//                                    ""ChatId"": ""{chatid}"",
//                                    ""Question"": ""{q}"",
//                                    ""DataId"": ""{fileid}""
//                                }}";

//    var content = new StringContent(jsonContent, null, "application/json");
//    var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7168/api/Similar");
//    request.Content = content;

//    var response = await client.SendAsync(request);

//    response.EnsureSuccessStatusCode();


//    return Json(response);
//}

//public class Setting
//{
//    public string ApplicationId { get; set; }
//    public string FileId { get; set; }
//}

//    }
//}
