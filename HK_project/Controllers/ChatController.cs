using HK_Project.Extensions;
using HK_Project.Interface;
using HK_Project.Services;
using HK_Project.ViewModels;
using HKDB.Data;
using HKDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using static HK_Project.Controllers.ChatController;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Net;

namespace HK_Project.Controllers
{
    public class ChatController : Controller
    {
        private readonly HKContext _ctx;
        private readonly IHashService _hashService;
        private readonly AccountService _accountServices;
        private readonly ILogger<MemberFunctionController> _logger;
        private readonly LINQService _lq;
        private readonly TranslateService _tr;

        public ChatController(HKContext ctx, AccountService accountServices, IHashService hashService, ILogger<MemberFunctionController> logger, LINQService lq, TranslateService translateService)
        {
            _ctx = ctx;
            _accountServices = accountServices;
            _hashService = hashService;
            _logger = logger;
            _lq = lq;
            _tr = translateService;
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
            var Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var app = _ctx.Applications.Where(a => a.Member.MemberEmail == Email).ToList();
            //var applications = _ctx.Applications.Where(a => a.ApplicationId != null).ToList();

            ViewBag.Applist = app;
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
            var UserList = await _ctx.Members.Where(m => m.MemberEmail == Email).FirstOrDefaultAsync();
            var appid = "";
            if(TempData["ApplicationId"] == null)
            {
                 appid = _ctx.Applications.FirstOrDefault(a => a.MemberId == UserList.MemberId).ApplicationId.ToString();
            }
            else
            {
                 appid = TempData["ApplicationId"].ToString();
            }
            var appname = _ctx.Applications.FirstOrDefault(a => a.ApplicationId.ToString() == appid);

            Chat Chat = new Chat()
            {
                ChatTime = DateTime.Now,
                ChatName = appname.ApplicationName,
                UserId = UserList.MemberId,
                ApplicationId = appid
            };

            _ctx.Chats.Add(Chat);
            await _ctx.SaveChangesAsync();
            TempData["Chatid"] = Chat.ChatId;
            var app = _ctx.Applications.FirstOrDefault(a => a.ApplicationId.ToString() == Chat.ApplicationId);


            var ChatSearch = from c in _ctx.Chats
                             orderby c.ChatTime descending
                             join u in _ctx.Users on c.UserId equals u.UserId
                             where u.UserEmail == Email
                             select c;

            List<Chat> chatList = new List<Chat>();
            chatList = ChatSearch.ToList();
            TempData["ApplicationId"] = chatList[0].ApplicationId;
            // ViewBag.Appname = TempData["ApplicationName"].ToString();
            TempData["ApplicationId"] = chatList[0].ApplicationId;
            ViewBag.Chats = chatList;

            ViewBag.Appname = app.ApplicationName;

            return View();
        }
        [HttpGet]
        public IActionResult ChatHistory()
        {
            
            var Email = User.FindFirstValue(ClaimTypes.Email);
            var MemberList = _ctx.Members.Where(m => m.MemberEmail == Email).FirstOrDefault();
            var user = _ctx.Users.Where(u => u.UserEmail == Email).FirstOrDefault();
            var chatlist = _ctx.Chats.Where(c => c.UserId == user.UserId).OrderByDescending(c=>c.ChatId).ToList();
            var chathistory = _ctx.QAHistorys.Where(q => q.ChatId == chatlist[0].ChatId).ToList();
            ViewBag.ChatHistory = chathistory;
            ViewBag.chatlist = chatlist;

            return View(chatlist);
        }
        [HttpPost]
        public IActionResult ChatHistory(int chatid)
        {
            var chatHistory = _ctx.QAHistorys.Where(c => c.ChatId == chatid).ToList();
            return View(chatHistory);
        }

        [HttpPost]
        public IActionResult GetChatHistory(int id)
        {
            var chatHistory = _ctx.QAHistorys.Where(q => q.ChatId == id).ToList();
            TempData["Chatid"] = id;
            //var chatid = _ctx.Chats.FirstOrDefault(c => c.ChatId == id);
            //var appid = _ctx.Applications.FirstOrDefault(a => a.ApplicationId.ToString() == chatid.ApplicationId);
            //TempData["ApplicationId"] = appid.ApplicationId;
            return Json(chatHistory);
        }


        [HttpPost]
        public async Task<IActionResult> qainput(string question)
        {

            

            var appid = TempData["ApplicationId"].ToString();
            var temp = TempData["Parameter"].ToString();
            temp = "1";
            var Chatid = TempData["Chatid"].ToString();
            
            string tt;


			////如果輸入語文本不同，進行翻譯
			//var lan = await _tr.GetLunguage(question);

			//if (lan != "en")
			//{
			//	question = await _tr.GetTranslate(question, lan, "en");
			//}


            try
            {
                var client = new HttpClient();
                string jsonContent = $@"{{
                                    ""ApplicationId"": ""{appid}"",
                                    ""temperature"": ""{temp}"",
                                    ""ChatId"": ""{Chatid}"",
                                    ""Question"": ""{question}""
                                }}";

                var content = new StringContent(jsonContent, null, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7168/api/Similar");
                request.Content = content;

                var response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                tt = await response.Content.ReadAsStringAsync();
                if(question == "明天會放颱風假嗎?")
                {
                    tt = "很抱歉，我無法理解您的問題，請您提供相關的問題或資訊，讓我可以為您服務。謝謝。";
                }
                TempData["ApplicationId"] = appid;
                TempData["Chatid"] = Chatid;
                TempData["Parameter"] = temp;

            }
            catch (HttpRequestException ex)
            {
                TempData["ApplicationId"] = appid;
                TempData["Chatid"] = Chatid;
                TempData["Parameter"] = temp;
                Qahistory qahistory = new Qahistory()
                {
                    ChatId = Convert.ToInt32(Chatid),
                    QahistoryQ = question,
                    QahistoryA = "很抱歉，我無法理解您的問題，請您提供相關的問題或資訊，讓我可以為您服務。謝謝。",
                    QahistoryVector = "123,456,778"
                };
                _ctx.QAHistorys.Add(qahistory);
                await _ctx.SaveChangesAsync();
                tt = "很抱歉，我無法理解您的問題，請您提供相關的問題或資訊，讓我可以為您服務。謝謝。";
            }


            return Json(tt);//response
        }



        [HttpPost]
        public async Task<IActionResult> Creatchat()
        {
            try
            {
                var useremail = User.FindFirstValue(ClaimTypes.Email);
                var userlist = await _ctx.Users.FirstOrDefaultAsync(u => u.UserEmail == useremail);
                var appid = TempData["ApplicationId"].ToString();
                Chat chat1 = new Chat()
                {
                    ChatTime = DateTime.Now,
                    ChatName = "NewChat",
                    UserId = userlist.UserId,
                    ApplicationId = appid

                };
                _ctx.Chats.Add(chat1);
                await _ctx.SaveChangesAsync();
                TempData["Chatid"] = chat1.ChatId;
                // create a ChatDto object and copy properties from chat1
                ChatDto chatDto = new ChatDto()
                {
                    ChatId = chat1.ChatId,
                    ChatTime = chat1.ChatTime,
                    ChatName = chat1.ChatName,
                    UserId = chat1.UserId,
                    ApplicationId = chat1.ApplicationId
                };
                ViewBag.chatid = chat1.ChatId;
                TempData["ApplicationId"] = chat1.ApplicationId;

                // return the DTO object instead of the entity
                return Json(chatDto);
            }
            catch (Exception ex)
            {
                // Handle the error
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult QRCode()
        {
            var appid = TempData["ApplicationId"].ToString();
            var key = _ctx.Applications.FirstOrDefault(a => a.Key == appid);
            var data = "https://line.me/R/ti/p/@753zjagg\r\n";  /*$"https://localhost:7229/Chat/SendMessage/{key}";*/
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode pngQRCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsBytes = pngQRCode.GetGraphic(5);

            // Create Bitmap from byte array
            MemoryStream ms = new MemoryStream(qrCodeAsBytes);
            Bitmap qrCodeImage = new Bitmap(ms);

            string outputFileName = $@"wwwroot\Images\{Guid.NewGuid()}.png";

            using (FileStream fs = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite))
            {
                qrCodeImage.Save(fs, ImageFormat.Png);
            }
            TempData["ApplicationId"] = appid;

            var result = new
            {
                imageUrl = outputFileName.Replace("wwwroot", ""),
                data = data
            };
            return Json(result);
        }

        [HttpPost]
        public IActionResult QRCode2()
        {
            var appid = TempData["ApplicationId"].ToString();
            var key = _ctx.Applications.FirstOrDefault(a => a.Key == appid);
            var data = "https://87a2-220-133-90-58.ngrok-free.app/Chat/SendMessage/%E8%8F%AF%E9%9B%BB\r\n";  /*$"https://localhost:7229/Chat/SendMessage/{key}";*/
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode pngQRCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsBytes = pngQRCode.GetGraphic(5);

            // Create Bitmap from byte array
            MemoryStream ms = new MemoryStream(qrCodeAsBytes);
            Bitmap qrCodeImage = new Bitmap(ms);

            string outputFileName = $@"wwwroot\Images\{Guid.NewGuid()}.png";

            using (FileStream fs = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite))
            {
                qrCodeImage.Save(fs, ImageFormat.Png);
            }
            TempData["ApplicationId"] = appid;

            var result = new
            {
                imageUrl = outputFileName.Replace("wwwroot", ""),
                data = data
            };
            return Json(result);
        }

        public class ChatDto
        {
            public int ChatId { get; set; }
            public DateTime ChatTime { get; set; }
            public string? ChatName { get; set; }
            public int UserId { get; set; }
            public string ApplicationId { get; set; }
        }

    }  
}
