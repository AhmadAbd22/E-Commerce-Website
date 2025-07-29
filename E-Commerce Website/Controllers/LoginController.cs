using Microsoft.AspNetCore.Mvc;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Models.Helping_Classes;

namespace ECommerceWebsite.Controllers
{
    public class LoginController : Controller
    {
        private readonly ECommerceWebsiteDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginController(ECommerceWebsiteDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            //checking if fields are null
            if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
            {
                ViewData["LoginError"] = "Username and Password are required.";
                return View(ViewData);
            }


            var user = _context.Users.FirstOrDefault(u => u.Username == loginDto.Username);
            if (user == null)
            {
                ViewData["LoginError"] = "Invalid username or User Doesn't Exist";
                return View(loginDto);
            }

            // hash enterd password and then compare with the stored password
            string hashedInputPassword = PasswordHelper.HashPassword(loginDto.Password);
            if (user.Password != hashedInputPassword)
            {
                TempData["Error"] = "Incorrect password.";
                return View(loginDto);
            }

            var auth  = new Authorization(_httpContextAccessor);
            await auth.SetUserClaims(user);

            return RedirectToAction("UserHome", "UserHome");
        }
    }
}
