using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWebsite.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0.0m;

        [Column(TypeName = "nvarchar(max)")] 
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}