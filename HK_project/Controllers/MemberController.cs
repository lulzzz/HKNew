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
        private readonly AccountService _accountServices;
        private readonly ILogger<MemberController> _logger;

        public MemberController(HKContext ctx, AccountService accountServices, IHashService hashService, ILogger<MemberController> logger)
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
                //var MemberEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                //var Member = _ctx.Members.FirstOrDefault(m => m.MemberEmail == MemberEmail);
                //var app = _ctx.Applications.Where(a => a.MemberId == Member.MemberId).ToList();

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
                        
                        Aifile embs = new Aifile()
                        {
                            AifileType = fileType,
                            AifilePath = filePath,
                            ApplicationId = short.Parse(TempData["ApplicationIdChooseapp"].ToString())
                        };

                        _ctx.Add(embs);
                        await _ctx.SaveChangesAsync();
                    }
                }

                //TempData["UploadSuccess"] = true;
                return RedirectToAction("Index", "Member");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload Error");
                return BadRequest();
            }
        }

    }
}
