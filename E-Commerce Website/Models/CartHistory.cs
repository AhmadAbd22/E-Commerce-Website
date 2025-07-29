using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWebsite.Models
{
    public class CartHistory : BaseModel
    {
        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAtPurchase { get; set; }

        [ForeignKey("UserId")]
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey("BookId")]
        public Guid? BookId { get; set; }
        public Book? Book { get; set; }

        [ForeignKey("OrderItemId")]
        public Guid? OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }
    }
}
