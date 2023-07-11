using Microsoft.AspNetCore.Mvc;
using HK_Project.ViewModels;
using HKDB.Models;
using HKDB.Data;
using System.Security.Claims;
using HK_Project.Services;

namespace HK_Project.Controllers
{
    public class UserManageController : Controller
    {
        private readonly HKContext _ctx;
        private readonly ClaimService _claim;
        public UserManageController(HKContext ctx, ClaimService claim)
        {
            _ctx = ctx;
            _claim = claim;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> UserLoginSingup(UserLoginSinginViewModel lvm)
        {
            if (ModelState.IsValid)
            {
                var UserEmail_exist = _ctx.Users.FirstOrDefault(u => u.UserEmail == lvm.Email);

                if(UserEmail_exist == null)
                {
                    
                    User user = new User()
                    {
                        UserEmail = lvm.Email
                    };
                    _ctx.Users.Add(user);
                    await _ctx.SaveChangesAsync();
                }
                await _claim.ClaimAdd(lvm.Email);
                
                return View();
            }
            return View(lvm);
        }





    }
}
