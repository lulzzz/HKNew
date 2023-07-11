using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using HK_Project.Interface;
using HKDB.Data;
using HKDB.Models;
using HK_Project.ViewModels;

namespace HK_Project.Services
{
    public class AccountService
    {

        private readonly HKContext _ctx;
        private readonly IHashService _hashService;

        public AccountService(HKContext context, IHashService hashService)
        {
            _ctx = context;
            _hashService = hashService;
        }

        public async Task<PasswordLSViewModel?> AuthenticateMember(PasswordLSViewModel loginVM) // member
        {
            var member = await _ctx.Members
                .FirstOrDefaultAsync(u => u.MemberEmail == loginVM.Email && u.MemberPassword == _hashService.MD5Hash(loginVM.Password));

            if (member != null)
            {
                var UserInfo = new PasswordLSViewModel
                {
                    Email = loginVM.Email,
                    Password = loginVM.Password,
                };

                 //Member userInfo = new Member
                 //{
                 //    MemberName = member.MemberId,
                 //    MemberEmail = loginVM.Email,
                 //    MemberPassword = loginVM.Password,
                 //};
                return UserInfo;
            }
            else
            {
                return null;
            }
        }

        public async Task<PasswordLSViewModel?> AuthenticateUser(PasswordLSViewModel loginVM) // user
        {
            //find user
            // _hashService.MD5Hash(loginVM.Password)
            var user = await _ctx.Users
                .FirstOrDefaultAsync(u => u.UserEmail.ToUpper() == loginVM.Email.ToUpper() && u.UserPassword == _hashService.MD5Hash(loginVM.Password));

            if (user != null)
            {
                var UserInfo = new PasswordLSViewModel
                {
                    Email = loginVM.Email,
                    Password = loginVM.Password
                };

                return UserInfo;
            }
            else
            {
                return null;
            }
        }
    }
}
