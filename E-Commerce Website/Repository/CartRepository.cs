using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebsite.Repository
{
    public interface ICartRepository
    {
        Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId);
        Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId);
        Task AddToCartAsync(CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(Guid cartItemId);
        Task ClearCartAsync(Guid userId);
        Task<int> GetCartItemCountAsync(Guid userId);
        Task<decimal> GetCartTotalAsync(Guid userId);
    }

    public class CartRepository : ICartRepository
    {
        private readonly ECommerceWebsiteDbContext _context;

        public CartRepository(ECommerceWebsiteDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Book)
                .OrderBy(ci => ci.CreatedAt)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Book)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
        }

        public async Task AddToCartAsync(CartItem cartItem)
        {
            if (cartItem == null)
            {
                throw new ArgumentNullException(nameof(cartItem), "Cart item cannot be null");
            }

            var book = await _context.Books.FindAsync(cartItem.BookId);
            if (book == null)
            {
                throw new InvalidOperationException("Book not found");
            }

            if (book.StockQuantity < cartItem.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock available");
            }

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == cartItem.UserId && ci.BookId == cartItem.BookId);

            if (existingCartItem != null)
            {
                // Check if total quantity would exceed stock
                var totalQuantity = existingCartItem.Quantity + cartItem.Quantity;
                if (totalQuantity > book.StockQuantity)
                {
                    throw new InvalidOperationException("Total quantity would exceed available stock");
                }

                existingCartItem.Quantity = totalQuantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
                _context.CartItems.Update(existingCartItem);
            }
            else
            {
                cartItem.Id = Guid.NewGuid();
                await _context.CartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            if (cartItem == null)
            {
                throw new ArgumentNullException(nameof(cartItem), "Cart item cannot be null");
            }

            var existingCartItem = await _context.CartItems
                .Include(ci => ci.Book)
                .FirstOrDefaultAsync(ci => ci.Id == cartItem.Id);

            if (existingCartItem == null)
            {
                throw new InvalidOperationException("Cart item not found");
            }

            // Validate stock availability
            if (existingCartItem.Book != null && cartItem.Quantity > existingCartItem.Book.StockQuantity)
            {
                throw new InvalidOperationException("Insufficient stock available");
            }

            existingCartItem.Quantity = cartItem.Quantity;
            existingCartItem.UpdatedAt = DateTime.UtcNow;

            _context.CartItems.Update(existingCartItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(Guid cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);

            if (cartItem == null)
            {
                throw new InvalidOperationException("Cart item not found");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetCartItemCountAsync(Guid userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }

        public async Task<decimal> GetCartTotalAsync(Guid userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Book)
                .SumAsync(ci => ci.Quantity * ci.Book.Price);
        }
    }
}