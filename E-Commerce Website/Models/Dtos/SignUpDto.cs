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

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.EmailAddress)]
        public string ConfirmEmail { get; set; }
    }

}
