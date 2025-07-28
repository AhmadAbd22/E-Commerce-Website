using System.ComponentModel.DataAnnotations;

namespace ECommerceWebsite.Models
{
    public class Author : BaseModel
    {
        [Required]
        public string AuthorName { get; set; } = string.Empty;

        public ICollection<Book>? Books { get; set; }

    }
}
