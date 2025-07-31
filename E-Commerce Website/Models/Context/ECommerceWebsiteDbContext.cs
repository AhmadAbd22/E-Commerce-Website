using Microsoft.EntityFrameworkCore;
using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Enums;
using ECommerceWebsite.Models.Helping_Classes;

namespace ECommerceWebsite.Models.Context
{
    public class ECommerceWebsiteDbContext : DbContext
    {
        public ECommerceWebsiteDbContext(DbContextOptions<ECommerceWebsiteDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    FirstName = "Admin",
                    LastName = "Ad",
                    Username = "Admin",
                    PhoneNumber = "01233456777",
                    Email = "admin@yopmail.com",
                    Password = PasswordHelper.HashPassword("123"),
                    Role = (int)enumRole.Admin,
                    CreatedAt = new DateTime(2024, 01, 01),
                    Address = "Johar",
                    City = "Lahore",
                    PostalCode = "55000",
                    DateOfBirth = new DateTime(2012,02,01),
                }

            );
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartHistory> CartHistories { get; set; }

    }
}
