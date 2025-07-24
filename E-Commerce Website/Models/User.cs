using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThemedApp.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.EmailAddress)]
        [Column(TypeName = "nvarchar(max)")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.EmailAddress)]
        [Column(TypeName = "nvarchar(255)")]
        public string Email { get; set; } = string.Empty;
    }
}
