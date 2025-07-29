using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWebsite.Models
{
    public class OrderItem : BaseModel
    {
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [ForeignKey("UserId")]
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey("OrderDetailsId")]
        public Guid? OrderDetails { get; set; }
        public OrderDetails? Orderetails { get; set; }  

        [ForeignKey("BookId")]
        public Guid? BookId { get; set; }
        public Book? Book { get; set; }
    }
}
