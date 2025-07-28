using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWebsite.Models
{
    public class User : BaseModel
    {
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Column(TypeName = "nvarchar(max)")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.EmailAddress)]
        [Column(TypeName = "nvarchar(255)")]
        public string Email { get; set; } = string.Empty;


        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(15)")]
        public string City { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]   
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(15)")]
        public string Province { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(5)")]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.PhoneNumber)]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone number must be exactly 11 digits.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must contain only digits.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
