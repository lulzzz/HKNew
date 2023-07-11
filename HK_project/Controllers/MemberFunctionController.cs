using HK_Project.Services;
using HK_Project.Interface;
using HKDB.Data;
using HKDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                    Model = "gpt-35-turbo",
                    Parameter = a.Parameter,
                    MemberId = a.MemberId,
                    ApplicationName = a.ApplicationName
                };

                _ctx.Add(app); // Use Add() here instead of Update()
                await _ctx.SaveChangesAsync();
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
                //抓到目前最大的ApplicationId
                var AppWithMaxId = await _ctx.Applications.OrderByDescending(u => u.ApplicationId).FirstOrDefaultAsync();

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
                            ApplicationId = AppWithMaxId.ApplicationId,
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

    }
}
