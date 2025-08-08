using Microsoft.AspNetCore.Mvc;
using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Models.Helping_Classes;
using ECommerceWebsite.Repository;
using ECommerceWebsite.Models.Enums;

namespace ECommerceWebsite.Controllers
{
    public class SignUpController : Controller
    {
        private readonly IUserRepository _userRepo;

        public SignUpController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
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
                string.IsNullOrEmpty(signupDto.Email))
            {
                ViewData["UsernameError"] = "All fields are required.";
                return View(signupDto);
            }

            var existingUser = await _userRepo.GetUserByEmailAsync(signupDto.Email);
            if(existingUser != null)
            {
                ViewData["ExistUser"] = "User already exists with this email. Use another email";
                return View(signupDto);
            }
            
            if (signupDto.Password != signupDto.ConfirmPassword)
            {
                ViewData["PasswordError"] = "Passwords do not match.";
                return View(signupDto);
            }

            //Create new User with proper initialization
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = signupDto.Username,
                FirstName = signupDto.FirstName,
                LastName = signupDto.LastName,
                Password = PasswordHelper.HashPassword(signupDto.Password),
                Email = signupDto.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Role = (int)enumRole.User,
                IsDeleted = false,
                isActive = (int)enumStatus.Active,
                // Set default values for required fields that aren't in SignUpDto
                Address = "",
                City = "",
                Province = "",
                PostalCode = "",
                PhoneNumber = "00000000000", // Default 11-digit phone number
                DateOfBirth = DateTime.UtcNow.AddYears(-18) // Default to 18 years ago
            };

            await _userRepo.AddUserAsync(user);

            TempData["Message"] = "Sign-up successful!";
            return RedirectToAction("Login", "Login");
        }
    }
}
