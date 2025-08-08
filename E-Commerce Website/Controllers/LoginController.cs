using Microsoft.AspNetCore.Mvc;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Models.Helping_Classes;
using ECommerceWebsite.Models.Enums;
using ECommerceWebsite.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace ECommerceWebsite.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginController(IUserRepository userRepo, IHttpContextAccessor httpContextAccessor)
        {
            _userRepo = userRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Login()
        {
            ViewData["isLoginView"] = true; 
            return View(new LoginDto());
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
                return View(loginDto);
            }

            // Use repository instead of direct DbContext access
            var user = await _userRepo.GetUserByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                ViewData["LoginError"] = "Invalid username or User Doesn't Exist";
                return View(loginDto);
            }

            if (user.IsDeleted)
            {
                TempData["LoginError"] = "Your account has been deactivated. Please contact support.";
                return View(loginDto);
            }

            // hash entered password and then compare with the stored password
            string hashedInputPassword = PasswordHelper.HashPassword(loginDto.Password);
            if (user.Password != hashedInputPassword)
            {
                TempData["Error"] = "Incorrect password.";
                return View(loginDto);
            }

            // Use Authorization helper for setting claims
            var auth = new Authorization(_httpContextAccessor);
            await auth.SetUserClaims(user);

            if (user.Role == (int)enumRole.Admin)
            {
                TempData["Success"] = "Welcome Back, Admin!";
                return RedirectToAction("Admin", "Admin");
            }
            else
            {
                TempData["Success"] = $"Welcome Back, {user.FirstName}!";
                return RedirectToAction("UserHome", "UserHome");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            ViewData["Login"] = false; 
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "You have been successfully logged out.";
            return RedirectToAction("Login", "Login");
        }
    }
}
