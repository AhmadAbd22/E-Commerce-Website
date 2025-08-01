using System.ComponentModel.DataAnnotations;

namespace ECommerceWebsite.Models
{
    public class BaseModel
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; } = null;
        public string CreatedBy { get; set; } = string.Empty;
        public int? Role { get; set; }
        public int? isActive { get; set; }

    }
}
