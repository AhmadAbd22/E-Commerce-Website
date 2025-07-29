using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWebsite.Models
{
    public class OrderDetails : BaseModel
    {
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public string OrderStatus { get; set; } = "Pending"; // Pending, Confirmed, Shipped, Delivered, Cancelled

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string City { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(15)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
