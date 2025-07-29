using ECommerceWebsite.Models;

namespace ECommerceWebsite.DTOs
{
    public class UserHomeDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public List<CartItem> CartItems { get; set; } = new();
        public List<OrderDetails> RecentOrders { get; set; } = new();
    }
}
