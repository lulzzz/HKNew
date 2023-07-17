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

            var appid = TempData["ApplicationId"].ToString();

            Chat Chat = new Chat()
            {
                ChatTime = DateTime.Now,
                ChatName = "NewChat",
                UserId = UserList.UserId,
                ApplicationId = appid
            };

            _ctx.Chats.Add(Chat);
            await _ctx.SaveChangesAsync();

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
            TempData["Chatid"] = id;
            return Json(chatHistory);
        }


        [HttpPost]
        public async Task<IActionResult> qainput(string question)
        {

            var appid = TempData["ApplicationId"].ToString();
            var temp = TempData["Parameter"].ToString();
            temp = "1";
            var Chatid = TempData["Chatid"].ToString();
            var q = question;

            var client = new HttpClient();
            string jsonContent = $@"{{
                                    ""ApplicationId"": ""{appid}"",
                                    ""temperature"": ""{temp}"",
                                    ""ChatId"": ""{Chatid}"",
                                    ""Question"": ""{q}""
                                }}";

            var content = new StringContent(jsonContent, null, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7168/api/Similar");
            request.Content = content;

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            var tt =await response.Content.ReadAsStringAsync();
            TempData["ApplicationId"] = appid;
            TempData["Chatid"] = Chatid;

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
                TempData["ApplicationId"]= chat1.ApplicationId;

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
            var data = "https://bootstrap5.hexschool.com/docs/5.1/components/modal/";  /*$"https://localhost:7229/Chat/SendMessage/{key}";*/
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
