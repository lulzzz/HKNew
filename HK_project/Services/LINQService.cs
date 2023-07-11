using HKDB.Data;
using HKDB.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HK_Project.Services
{
    public class LINQService
    {
        private readonly HKContext _ctx;
        public LINQService(HKContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<User?> GetUser(string email)
        {
            var result = await _ctx.Users.FirstOrDefaultAsync(u => u.UserEmail == email);
            return result;
        }

        public async Task<Member?> GetMember(string email)
        {
            var result = await _ctx.Members.FirstOrDefaultAsync(u => u.MemberEmail == email);
            return result;
        }

        public async Task<List<Application?>> GetApplication(string email)
        {
            var temp = from a in _ctx.Applications
                       join m in _ctx.Members on a.MemberId equals m.MemberId
                       where m.MemberEmail == email
                       select a;

            var result =await temp.ToListAsync();

            return result;
        }
    }
}
