using ECommerceWebsite.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Book : BaseModel
{
    [Required]
    [Column(TypeName = "nvarchar(100)")]
    [Display(Name = "Book Name")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public Guid AuthorId { get; set; } 

    [ForeignKey("AuthorId")]
    public Author? Author { get; set; } 

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; } = 0.0m;

    [Required]
    public string ISBN { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Publication Date")]
    public DateTime? PublicationDate { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative integer.")]
    [Required]
    [Column(TypeName = "int")]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }


    [Column(TypeName = "nvarchar(max)")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(255)")]
    [Display(Name = "Image URL")]
    public string ImageUrl { get; set; } = string.Empty;

    // Category Relationship (One-to-Many)
    [Required]
    public Guid CategoryId { get; set; } // FK column

    [ForeignKey("CategoryId")]
    public Category? Category { get; set; } // Navigation property
}
