using System.ComponentModel.DataAnnotations;

namespace ECommerceWebsite.Models.Dtos
{
    public class CartDto
    {
        public List<CartItemDto> CartItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
    }

    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public decimal BookPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int StockAvailable { get; set; }
    }

    public class CheckoutDto
    {
        public List<CartItemDto> CartItems { get; set; } = new();
        public decimal TotalAmount { get; set; }

        [Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
