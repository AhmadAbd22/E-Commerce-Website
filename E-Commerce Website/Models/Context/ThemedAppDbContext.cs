using Microsoft.EntityFrameworkCore;
using ECommerceWebsite.Models;
namespace ECommerceWebsite.Models.Context
{
        public class ECommerceWebsiteDbContext : DbContext
    {
        public ECommerceWebsiteDbContext(DbContextOptions<ECommerceWebsiteDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
