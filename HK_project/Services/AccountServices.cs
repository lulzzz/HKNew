using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using HK_project.Interface;
using HKDB.Data;
using HKDB.Models;
using HK_project.ViewModels;

namespace HK_Product.Services
{
    public class AccountServices
    {

        private readonly HKContext _ctx;
        private readonly IHashService _hashService;

        public AccountServices(HKContext context, IHashService hashService)
        {
            _ctx = context;
            _hashService = hashService;
        }

        public async Task<Member?> AuthenticateMember(LoginViewModel loginVM)
        {
            var member = await _ctx.Members
                .FirstOrDefaultAsync(u => u.MemberEmail.ToUpper() == loginVM.Email.ToUpper() && u.MemberPassword == _hashService.MD5Hash( loginVM.Password));

            if (member != null)
            {
                Member userInfo = new Member
                {
                    MemberId = member.MemberId,
                    MemberName = member.MemberId.ToString(),
                    MemberEmail = loginVM.Email,
                    MemberPassword = loginVM.Password,
                };

                return userInfo;
            }
            else
            {
                return null;
            }
        }

        public async Task<User?> AuthenticateUser(LoginViewModel loginVM)
        {
            //find user
            // _hashService.MD5Hash(loginVM.Password)
            var user = await _ctx.Users
                .FirstOrDefaultAsync(u => u.UserEmail.ToUpper() == loginVM.Email.ToUpper() && u.UserPassword == _hashService.MD5Hash(loginVM.Password));

            if (user != null)
            {
                User userInfo = new User
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserEmail = loginVM.Email,
                    UserPassword = loginVM.Password
                };

                return userInfo;
            }
            else
            {
                return null;
            }
        }
    }
}
