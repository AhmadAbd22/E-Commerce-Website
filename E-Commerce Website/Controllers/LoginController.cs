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
                TempData.SetError("Username and Password are required.");
                return View(loginDto);
            }

            var user = await _userRepo.GetUserByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                TempData.SetError("Invalid username or User Doesn't Exist");
                return View(loginDto);
            }

            if (user.IsDeleted)
            {
                TempData.SetWarning("Your account has been deactivated. Please contact support.");
                return View(loginDto);
            }

            // hash entered password and then compare with the stored password
            string hashedInputPassword = PasswordHelper.HashPassword(loginDto.Password);
            if (user.Password != hashedInputPassword)
            {
                TempData.SetError("Incorrect password. Please try again.");
                return View(loginDto);
            }

            // Use Authorization helper for setting claims
            var auth = new Authorization(_httpContextAccessor);
            await auth.SetUserClaims(user);

            if (user.Role == (int)enumRole.Admin)
            {
                TempData.SetSuccess($"Welcome Back, {user.FirstName}");
                return RedirectToAction("Admin", "Admin");
            }
            else
            {
                TempData.SetSuccess($"Welcome Back, {user.FirstName} {user.LastName}!");
                return RedirectToAction("UserHome", "UserHome");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            ViewData["Login"] = false; 
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData.SetInfo("You have been successfully logged out.");
            return RedirectToAction("Login", "Login");
        }
        public async Task<IActionResult> DeleteAccount()
        {
            var auth = new Authorization(_httpContextAccessor);
            Guid userId = auth.GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                TempData.SetError("Unable to identify user. Please log in again.");
                return RedirectToAction("Login", "Login");
            }
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
            {
                TempData.SetError("User not found.");
                return RedirectToAction("Login", "Login");
            }
            await _userRepo.DeleteUserAsync(userId);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData.SetSuccess("Your account has been successfully deleted.");
            return RedirectToAction("Login", "Login");
        }
    }
}
