using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebsite.Repository
{
    public interface ICartHistoryRepository
    {
        Task<IEnumerable<CartHistory>> GetUserOrderHistoryAsync(Guid userId);
        Task<IEnumerable<CartHistory>> GetOrderHistoryByOrderItemIdAsync(Guid orderItemId);
        Task AddCartHistoryAsync(CartHistory cartHistory);
        Task<CartHistory?> GetCartHistoryByIdAsync(Guid id);
    }

    public class CartHistoryRepository : ICartHistoryRepository
    {
        private readonly ECommerceWebsiteDbContext _context;

        public CartHistoryRepository(ECommerceWebsiteDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartHistory>> GetUserOrderHistoryAsync(Guid userId)
        {
            return await _context.CartHistories
                .Where(ch => ch.UserId == userId)
                .Include(ch => ch.Book)
                    .ThenInclude(b => b.Author)
                .Include(ch => ch.OrderItem)
                .OrderByDescending(ch => ch.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CartHistory>> GetOrderHistoryByOrderItemIdAsync(Guid orderItemId)
        {
            return await _context.CartHistories
                .Where(ch => ch.OrderItemId == orderItemId)
                .Include(ch => ch.Book)
                    .ThenInclude(b => b.Author)
                .ToListAsync();
        }

        public async Task AddCartHistoryAsync(CartHistory cartHistory)
        {
            if (cartHistory == null)
            {
                throw new ArgumentNullException(nameof(cartHistory));
            }

            cartHistory.Id = Guid.NewGuid();
            cartHistory.CreatedAt = DateTime.UtcNow;
            cartHistory.UpdatedAt = DateTime.UtcNow;

            await _context.CartHistories.AddAsync(cartHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<CartHistory?> GetCartHistoryByIdAsync(Guid id)
        {
            return await _context.CartHistories
                .Include(ch => ch.Book)
                    .ThenInclude(b => b.Author)
                .Include(ch => ch.OrderItem)
                .FirstOrDefaultAsync(ch => ch.Id == id);
        }
    }
}
