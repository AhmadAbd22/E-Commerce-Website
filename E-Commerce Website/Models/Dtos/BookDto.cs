using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Dtos;
using System.ComponentModel.DataAnnotations;

public class BookDto
{
    public Guid Id { get; set; }

    [Required]    
    public string Title { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Author Author { get; set; }
    public Guid AuthorId { get; set; } 
    public Category Category { get; set; }
    public Guid? CategoryId { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime? PublicationDate { get; set; }
    public List<Category> Categories { get; set; }
    public IEnumerable<Category> CategoriesList { get; set; }
}