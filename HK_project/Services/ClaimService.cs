using HK_Project.ViewModels;
using HKDB.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace HK_Project.Services
{
    public class ClaimService
    {
        private readonly HttpContext _httpContext;

        public ClaimService(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public async Task ClaimAdd(string Email)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, Email)
                };

            var claimsIdentity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await _httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }      
    }
}
