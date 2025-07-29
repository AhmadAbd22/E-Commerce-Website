using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWebsite.Models
{
    public class CartItem : BaseModel
    {
        public int Quantity { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }
        public Guid? BookId { get; set; }

        [ForeignKey("UserId")]
        public Guid? UserId { get; set; }
        public User? User { get; set; }
    }
}
    