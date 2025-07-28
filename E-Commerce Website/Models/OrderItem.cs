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

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid BookId { get; set; }


        [ForeignKey("OrderId")]
        public OrderDetails Orderetails { get; set; }  

        [ForeignKey("BookId")]
        public Book Book { get; set; }
    }
}
