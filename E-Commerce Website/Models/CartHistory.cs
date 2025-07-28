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

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid BookId { get; set; }

        [Required]
        public Guid OrderItemId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("BookId")]
        public Book Book { get; set; }

        [ForeignKey("OrderItemId")]
        public OrderItem OrderItem { get; set; }
    }
}
