using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWebsite.Models
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        [Display(Name = "Book Name")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        [Display(Name = "Author")]
        public string Author { get;  set; } = string.Empty;

        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0.0m;

        [Required]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Publication Date")]
        public DateTime PublicationDate { get; set; }


        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative integer.")]
        [Column(TypeName = "int")]
        [Required]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Column(TypeName = "nvarchar(max)")] 
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;
    }
}