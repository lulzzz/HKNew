using HK_Project.Services;
using HK_Project.Interface;
using HKDB.Data;
using HKDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using System;

namespace HK_Project.Controllers
{
    public class MemberFunctionController : Controller
    {

        private readonly HKContext _ctx;
        private readonly IHashService _hashService;
        private readonly AccountService _accountServices;
        private readonly ILogger<MemberFunctionController> _logger;
        private readonly LINQService _lq;

        public MemberFunctionController(HKContext ctx, AccountService accountServices, IHashService hashService, ILogger<MemberFunctionController> logger, LINQService lINQService)
        {
            _ctx = ctx;
            _accountServices = accountServices;
            _hashService = hashService;
            _logger = logger;
            _lq = lINQService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Createapp()
        {
            var Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;//
            var member = await _lq.GetMember(Email);

            ViewBag.MemberCreatapp = member;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Createapp(Application a) //更改為VModel
        {
            if (ModelState.IsValid)
            {
                Application app = new()
                {
                    Model = a.Model,
                    Parameter =a.Parameter,
                    MemberId = a.MemberId,
                    ApplicationName = a.ApplicationName
                };

                _ctx.Add(app); // Use Add() here instead of Update()
                await _ctx.SaveChangesAsync();
                TempData["ApplicationIdChooseapp"] = app.ApplicationId;

                return RedirectToAction("Uploadfileapp", "MemberFunction");
            }
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Chooseapp()
        {
            var Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var app = await _lq.GetApplication(Email);
            //var Member = _ctx.Members.FirstOrDefault(m => m.MemberEmail == MemberEmail);
            //var app = _ctx.Applications.Where(a => a.MemberId == Member.MemberId).ToList();

            ViewBag.AppChooseapp = app;
            return View();
        }
        [HttpPost]
        public IActionResult Chooseapp(int applicationId, string parameter)
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
                var appid = TempData["ApplicationIdChooseapp"].ToString();
                var app = _ctx.Applications.FirstOrDefault(a => a.ApplicationId.ToString() == appid);

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
                        
                        Aifile embs = new()
                        {
                            AifileType = fileType,
                            AifilePath = filePath,
                            ApplicationId = app.ApplicationId,
                        };

                        _ctx.Add(embs);
                        await _ctx.SaveChangesAsync();
                    }
                }

                //TempData["UploadSuccess"] = true;
                return RedirectToAction("MemberIndex", "Chat");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload Error");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var member = await _lq.GetMember(Email);
            ViewBag.MemberName = member.MemberName;
            ViewBag.MemberEmail = member.MemberEmail;
            ViewBag.MemberPassword = member.MemberPassword;

            var app = _ctx.Applications.Where(a => a.MemberId == member.MemberId).ToList();
            List<Application> appList = new();
            appList = app;
            ViewBag.AppList = appList;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Manage(string name, string password, string account)
        {
            var Member = await _lq.GetMember(account);

            if (Member != null)
            {
                if (Member.MemberPassword == password)
                {
                    Member.MemberName = name;
                }
                else
                {
                    Member.MemberPassword = _hashService.MD5Hash(password);
                    Member.MemberName = name;
                }
                _ctx.Update(Member);
                await _ctx.SaveChangesAsync();
            }
            return View();
        }
        [HttpPost]
        public IActionResult Appchooseresult(string ApplicationId, string Parameter, string ApplicationName)
        {
            TempData["ChooseAppId"] = ApplicationId;
            return Json(ApplicationId);
        }

        public IActionResult App()
        {
              return View();
        }
        [HttpGet]
        public IActionResult Appmanage()
        {
            var AppId = TempData["ChooseAppId"];
            var app = _ctx.Applications.FirstOrDefault(a => a.ApplicationId.ToString() == AppId);
            ViewBag.AppName = app.ApplicationName;
            ViewBag.model = app.Model;
            ViewBag.parameter = app.Parameter;

            var Aifile = _ctx.AiFiles.Where(a => a.ApplicationId.ToString() == AppId).ToList();
            ViewBag.Aifile = Aifile;

            TempData["ChooseAppId"] = AppId;
            return View();
        }

        //[HttpPost]
        //public IActionResult Appmanage()
        //{
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> Renewuploadfileapp(List<IFormFile> files)
        {
            string path = "Upload";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            try
            {
                var appid = TempData["ChooseAppId"].ToString();
                var app = _ctx.Applications.FirstOrDefault(a => a.ApplicationId.ToString() == appid);

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

                        Aifile embs = new()
                        {
                            AifileType = fileType,
                            AifilePath = filePath,
                            ApplicationId = app.ApplicationId,
                        };

                        _ctx.Add(embs);
                        await _ctx.SaveChangesAsync();
                    }
                }

                TempData["ChooseAppId"] = appid;
                //TempData["UploadSuccess"] = true;
                return RedirectToAction("Appmanage", "MemberFunction");

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload Error");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUpdatedTable(string appId)
        {
            var aifile = _ctx.AiFiles.Where(a => a.ApplicationId.ToString() == appId).ToList();
            return PartialView("_PartialViewFileName", aifile);
        }


        [HttpPost]
        public async Task<IActionResult> Reviseapp(string Model, string ApplicationName, string Parameter)
        {
            var appid = TempData["ChooseAppId"].ToString();
            var app = _ctx.Applications.FirstOrDefault(a => a.ApplicationId.ToString() == appid);
            if(Model!= null && ApplicationName != null && Parameter != null)
            {
                app.Model = Model;
                app.ApplicationName = ApplicationName;
                app.Parameter = Parameter;
                _ctx.Update(app);
                await _ctx.SaveChangesAsync();
            }
            TempData["ChooseAppId"] = appid;
            return Json(app);
        }


        [HttpPost]
        public async Task<IActionResult> Deletefile(int id)
        {
            var Aifile = _ctx.AiFiles.FirstOrDefault(a => a.AifileId == id);
            _ctx.Remove(Aifile);
            await _ctx.SaveChangesAsync();
            return Json("OK");
        }









    }
}
