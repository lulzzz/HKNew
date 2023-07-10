using HK_Project.ViewModels;
using HKDB.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace HK_Project.Services
{
    public class ClaimServer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimServer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ClaimViewModel> ClaimAdd(string Email)
        {


            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, Email)
                };

            var claimsIdentity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return null;
        }
    }
}
