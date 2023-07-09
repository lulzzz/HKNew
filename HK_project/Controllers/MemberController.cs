using HK_Project.Services;
using HK_Project.Interface;
using HKDB.Data;
using HKDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HK_Project.Controllers
{
    public class MemberController : Controller
    {

        private readonly HKContext _ctx;
        private readonly IHashService _hashService;
        private readonly AccountServices _accountServices;
        private readonly ILogger<MemberController> _logger;

        public MemberController(HKContext ctx, AccountServices accountServices, IHashService hashService, ILogger<MemberController> logger)
        {
            _ctx = ctx;
            _accountServices = accountServices;
            _hashService = hashService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Createapp()
        {
            var MemberEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;//
            var member = await _ctx.Members.FirstOrDefaultAsync(m => m.MemberEmail == MemberEmail);

            ViewBag.MemberCreatapp = member;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Createapp(Application a) //更改為VModel
        {
            if (ModelState.IsValid)
            {
                //var AppWithMaxId = await _ctx.Applications.OrderByDescending(u => u.ApplicationId).FirstOrDefaultAsync();
                //string newAppId;
                //if (AppWithMaxId != null)
                //{
                //    int maxId = int.Parse(AppWithMaxId.ApplicationId.Substring(1));

                //    int newId = maxId + 1;

                //    newAppId = "A" + newId.ToString().PadLeft(4, '0');

                //}
                //else
                //{
                //    newAppId = "A0001";
                //}
                Application app = new Application //View修改
                {
                    Model = "gpt-35-turbo",
                    Parameter = a.Parameter,
                    MemberId = a.MemberId,
                    ApplicationName = a.ApplicationName
                };

                _ctx.Add(app); // Use Add() here instead of Update()
                await _ctx.SaveChangesAsync();
                return RedirectToAction("Uploadfileapp", "Member");
            }
            return View();
        }


        [HttpGet]
        public IActionResult Chooseapp()
        {
            var MemberEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var Member = _ctx.Members.FirstOrDefault(m => m.MemberEmail == MemberEmail);
            var app = _ctx.Applications.Where(a => a.MemberId == Member.MemberId).ToList();
            ViewBag.AppChooseapp = app;
            return View();
        }
        [HttpPost]
        public IActionResult Chooseapp(string applicationId, string parameter)
        {
            ViewBag.ApplicationIdChooseapp = applicationId;
            TempData["ApplicationIdChooseapp"] = applicationId;
            ViewBag.ParameterChooseapp = parameter;
            //ViewBag.AppChooseapp = ViewBag.AppChooseapp;
            return RedirectToAction("Uploadfileapp", "Member");
        }

        [HttpGet]
        public IActionResult Uploadfileapp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Uploadfileapp(List<IFormFile> files)
        {
            string path = "Upload";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            try
            {
                // 獲取當前已驗證使用者的名稱
                var memberId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string filePath = Path.Combine(path, fileName);
                        string fileType = file.ContentType;

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // 取得完整檔案路徑
                        string fullPath = Path.GetFullPath(filePath);
                        _logger.LogInformation($"Upload file success, path: {fullPath}");

                        var fileWithMaxId = await _ctx.AiFiles.OrderByDescending(u => u.AifileId).FirstOrDefaultAsync();
                        //string id;

                        //if (fileWithMaxId.AifileId != null)
                        //{
                        //    // 從 UserID 中提取出數字部分，並轉換成整數
                        //    int maxId = int.Parse(fileWithMaxId.AifileId.Substring(1));
                        //    // 新的 UserID 是最大 UserID 加一
                        //    int newId = maxId + 1;
                        //    // 將新的 UserID 轉換回字符串形式，並確保它始終有四位數字
                        //    string newUserId = "D" + newId.ToString().PadLeft(4, '0');
                        //    id = newUserId;
                        //}
                        //else
                        //{
                        //    // 如果沒有任何 User，則新的 UserID 可以是 "U0001"
                        //    string newUserId = "D0001";
                        //    id = newUserId;
                        //}

                        Aifile embs = new Aifile()
                        {
                            AifileType = fileType,
                            AifilePath = filePath,
                        };

                        _ctx.Add(embs);
                        await _ctx.SaveChangesAsync();
                    }
                }

                TempData["UploadSuccess"] = true;
                return RedirectToAction("Index", "Member");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload Error");
                return BadRequest();
            }
        }
        [HttpGet]
        public async Task<IActionResult> Qa()
        {

            var memberId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var member = _ctx.Members.FirstOrDefault(m => m.MemberId.ToString() == memberId);
            var memberEmailinuser = _ctx.Users.FirstOrDefault(u => u.UserEmail == member.MemberEmail);
            
            if (memberEmailinuser == null)
            {
                //沒有聊天過  加入user chat
                //var useridmax = _ctx.Users.OrderByDescending(u => u.UserId).FirstOrDefault();
                //string userid;
                //if (useridmax != null)
                //{
                //    int maxId = int.Parse(useridmax.UserId.Substring(1)) + 1;
                //    string newUserId = "U" + maxId.ToString().PadLeft(4, '0');
                //    userid = newUserId;

                //}
                //else
                //{
                //    string newUserId = "U0001";
                //    userid = newUserId;
                //}
                User user = new User()
                {
                    UserName = "User",
                    UserEmail = member.MemberEmail,
                    UserPassword = member.MemberPassword
                };
                _ctx.Add(user);
                await _ctx.SaveChangesAsync();
                //var chatidmax = _ctx.Chats.OrderByDescending(u => u.UserId).FirstOrDefault();
                //string chatid;
                //if (chatidmax != null)
                //{
                //    int maxId = int.Parse(chatidmax.ChatId.Substring(1)) + 1;
                //    string newchatId = "C" + maxId.ToString().PadLeft(4, '0');
                //    chatid = newchatId;
                //}
                //else
                //{
                //    string newchatId = "C0001";
                //    chatid = newchatId;
                //}
                var userid = _ctx.Users.FirstOrDefault(u => u.UserEmail == member.MemberEmail).UserId;
                var chatid = "Chat_" + userid.ToString().PadLeft(4,'0');
                Chat chat = new Chat()
                {
                    ChatTime = DateTime.Now,
                    ChatName = chatid,
                    UserId = userid
                };
                _ctx.Add(chat);
                await _ctx.SaveChangesAsync();
                // 應用  id or name
                return View();

            }
            else
            {
                //抓chat歷史紀錄
                var userId = _ctx.Users.FirstOrDefault(u => u.UserEmail == member.MemberEmail).UserId;
                var chat = _ctx.Chats.Where(c => c.UserId == userId).OrderByDescending(c => c.ChatTime);
                ViewBag.ChatQa = chat;
                ViewBag.UserQa = userId;
                return View();
            }
        }
    }
}
