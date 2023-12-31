﻿using HK_Project.ViewModels;
using HKDB.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace HK_Project.Services
{
    public class ClaimService
    {
        public async Task<ClaimsIdentity> ClaimAdd(string Email)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, Email)
                };

            var claimsIdentity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            return claimsIdentity;
        }      
    }
}
