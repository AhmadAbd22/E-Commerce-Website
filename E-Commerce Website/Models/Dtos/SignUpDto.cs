using System.ComponentModel.DataAnnotations;

namespace ECommerceWebsite.Models.Dtos
{
    public class SignUpDto
    {
        public string Username { get; set; }
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        //[DataType(DataType.EmailAddress)]
        //public string ConfirmEmail { get; set; }

        //[DataType(DataType.PhoneNumber)]
        //[StringLength(11, MinimumLength = 11, ErrorMessage = "Phone number must be exactly 11 digits.")]
        //[RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must contain only digits.")]
        //public string PhoneNumber { get; set; }
        //public string? Address { get; set; }
        //public string? City { get; set; }
        //public string? Province { get; set; }

        //[DataType(DataType.PostalCode)]
        //public string? PostalCode { get; set; }

        //[DataType(DataType.Date)]
        //public DateTime DOB { get; set; }

    }

}
