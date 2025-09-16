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
            //Checking Conditions  (TODO: check separately after testing)
            if (string.IsNullOrEmpty(signupDto.Username) || string.IsNullOrEmpty(signupDto.Password) || 
                string.IsNullOrEmpty(signupDto.FirstName) || string.IsNullOrEmpty(signupDto.LastName) ||
                string.IsNullOrEmpty(signupDto.Email) )
            {
                TempData.SetError("All fields are required.");
                return View(signupDto);
            }

            var validEmail = await EmailValidationHelper.IsEmailValidAsync(signupDto.Email);

            if (!validEmail)
            {

                var existingUser = await _userRepo.GetUserByEmailAsync(signupDto.Email);
                if (existingUser != null)
                {
                    TempData.SetWarning("User already exists with this email. Use another email");
                    return View(signupDto);
                }

                if (signupDto.Password != signupDto.ConfirmPassword)
                {
                    TempData.SetError("Passwords do not match.");
                    return View(signupDto);
                }
            }
            else
            {
                TempData.SetError($"Invalid email format or email domain. Entered email {signupDto.Email}." );
                return View(signupDto);
            }


                //Create new User 
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
                    Role = (int)enumRole.Customer,
                    IsDeleted = false,
                    isActive = (int)enumStatus.Active,

                    //TODO: Uncomment these after chanegs in Singup.cshtml
                    //Address = signupDto.Address,
                    //City = signupDto.City,
                    //Province = signupDto.Province,
                    //PostalCode = signupDto.PostalCode,
                    //PhoneNumber = signupDto.PhoneNumber, 
                    //DateOfBirth = signupDto.DOB,
                };

            await _userRepo.AddUserAsync(user);

            TempData.SetSuccess("Sign-up successful! Please log in to continue.");
            return RedirectToAction("Login", "Login");
        }
    }
}
