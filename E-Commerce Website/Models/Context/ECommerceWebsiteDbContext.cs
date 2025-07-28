using Microsoft.EntityFrameworkCore;
using ECommerceWebsite.Models;
namespace ECommerceWebsite.Models.Context
{
    public class ECommerceWebsiteDbContext : DbContext
    {
        public ECommerceWebsiteDbContext(DbContextOptions<ECommerceWebsiteDbContext> options) : base(options) { }
        public DbSet<Author> Authors { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartHistory> CartHistories { get; set; }


    }
}
