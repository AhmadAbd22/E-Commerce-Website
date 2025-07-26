using System.ComponentModel.DataAnnotations;

namespace ECommerceWebsite.Models
{
    public class BaseModel
    {
        [Key]
        public Guid Id;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; } = null;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
