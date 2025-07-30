using System.ComponentModel.DataAnnotations;

namespace ECommerceWebsite.Models
{
    public class Category : BaseModel
    {
        [Required]
        public string CategoryType { get; set; } = string.Empty;
        public ICollection<Book>? Books { get; set; }
    }
}
