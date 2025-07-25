using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Models.Helping_Classes;

namespace ECommerceWebsite.Controllers
{
    public class SignUpController : Controller
    {
        private readonly ECommerceWebsiteDbContext _context;

        public SignUpController(ECommerceWebsiteDbContext context)
        {
            _context = context;
        }
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpDto signupDto)
        {
            if (!ModelState.IsValid)
            {
                return View(signupDto);
            }
            //Checking Conditions
            if (string.IsNullOrEmpty(signupDto.Username) || string.IsNullOrEmpty(signupDto.Password) || 
                string.IsNullOrEmpty(signupDto.FirstName) || string.IsNullOrEmpty(signupDto.LastName) ||
                string.IsNullOrEmpty(signupDto.Email) )
            {
                ViewData["UsernameError"] = "All fields are required.";
                return View(signupDto);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == signupDto.Email);
            if(existingUser != null)
            {
                ViewData["ExistUser"] = "User already exists with this email. Use another emal";
                return View(signupDto);
            }
            
            if (signupDto.Password != signupDto.ConfirmPassword)
            {
                ViewData["PasswordError"] = "Passwords do not match.";
                return View(signupDto);
            }

            //CReat new User 
            var user = new User
            {
                Username = signupDto.Username,
                FirstName = signupDto.FirstName,
                LastName = signupDto.LastName,
                Password = PasswordHelper.HashPassword(signupDto.Password),
                Email = signupDto.Email
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Sign-up successful!";
            return RedirectToAction("Login", "Login");
        }
    }
}
