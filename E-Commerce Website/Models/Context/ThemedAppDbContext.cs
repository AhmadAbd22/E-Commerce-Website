using Microsoft.EntityFrameworkCore;
using ThemedApp.Models;
namespace ThemedApp.Models.Context
{
        public class ThemedAppDbContext : DbContext
    {
        public ThemedAppDbContext(DbContextOptions<ThemedAppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
