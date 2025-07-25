
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using ECommerceWebsite.Models.Dtos;

namespace ECommerceWebsite.Models.Helping_Classes
{
    public class Authorization
    {
        private readonly HttpContext hcontext;
        public Authorization(IHttpContextAccessor haccess)
        {
            hcontext = haccess.HttpContext;
        }

        public UserDto? GetUserClaims()
        {
            string? userId = hcontext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            string? encId = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "EncId")?.Value;
            string? name = hcontext?.User.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
            string? email = hcontext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            UserDto? loggedInUser = null;
            if (userId != null)
            {
                loggedInUser = new UserDto()
                {
                    Id = userId,
                    EncId = encId,
                    Name = name,
                    Email = email,
                };
            }

            return loggedInUser;
        }

        public async Task<bool> SetUserClaims(User user)
        {
            try
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim("EncId", user.Id.ToString()),
                    new Claim("UserName", user.FirstName + " " + user.LastName),
                    new Claim(ClaimTypes.Email, user.Email),
                };


                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await hcontext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties { IsPersistent = true }
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static DateTime DateTimeNow()
        {
            return DateTime.UtcNow;
        }
    }
}