using ECommerceWebsite.Models;
using ECommerceWebsite.Repository;
using ECommerceWebsite.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebsite.Services
{
    public interface IOrderServiceRepository
    {
        Task<OrderDetails> PlaceOrderFromCartAsync(Guid userId, string shippingAddress, string city, string phoneNumber);
        Task<decimal> CalculateCartTotalAsync(Guid userId);
        Task<bool> ValidateCartStockAsync(Guid userId);
    }

    public class OrderServiceRepository : IOrderServiceRepository
    {
        private readonly ICartRepository _cartRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly ICartHistoryRepository _cartHistoryRepo;
        private readonly IBookRepository _bookRepo;
        private readonly ECommerceWebsiteDbContext _context;

        public OrderServiceRepository(
            ICartRepository cartRepo,
            IOrderRepository orderRepo,
            ICartHistoryRepository cartHistoryRepo,
            IBookRepository bookRepo,
            ECommerceWebsiteDbContext context)
        {
            _cartRepo = cartRepo;
            _orderRepo = orderRepo;
            _cartHistoryRepo = cartHistoryRepo;
            _bookRepo = bookRepo;
            _context = context;
        }

        public async Task<OrderDetails> PlaceOrderFromCartAsync(Guid userId, string shippingAddress, string city, string phoneNumber)
        {
            // ? FIXED: Use database transaction for proper order
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Get cart items
                var cartItems = await _cartRepo.GetCartItemsByUserIdAsync(userId);
                if (!cartItems.Any())
                {
                    throw new InvalidOperationException("Cart is empty");
                }

                // 2. Validate stock availability
                if (!await ValidateCartStockAsync(userId))
                {
                    throw new InvalidOperationException("Some items in your cart are no longer available in the requested quantity");
                }

                // 3. Calculate total
                var totalAmount = await CalculateCartTotalAsync(userId);

                // 4. Create Order Details
                var orderDetails = new OrderDetails
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TotalAmount = totalAmount,
                    OrderDate = DateTime.UtcNow,
                    OrderStatus = "Pending",
                    ShippingAddress = shippingAddress,
                    City = city,
                    PhoneNumber = phoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.OrderDetails.Add(orderDetails);

                var cartHistoriesToAdd = new List<CartHistory>();
                var orderItems = new List<OrderItem>();

                foreach (var cartItem in cartItems)
                {
                    if (cartItem.Book == null) continue;

                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        BookId = cartItem.BookId,
                        UserId = userId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Book.Price,
                        // OrderetailsId = orderDetails.Id, 
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    orderItems.Add(orderItem);

                    var cartHistory = new CartHistory
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        BookId = cartItem.BookId,
                        OrderItemId = orderItem.Id,
                        Quantity = cartItem.Quantity,
                        PriceAtPurchase = cartItem.Book.Price,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    cartHistoriesToAdd.Add(cartHistory);

                    // Update book stock
                    var book = await _bookRepo.GetBookByIdAsync(cartItem.BookId.Value);
                    if (book != null)
                    {
                        book.StockQuantity -= cartItem.Quantity;
                        book.UpdatedAt = DateTime.UtcNow;
                        _context.Books.Update(book);
                    }
                }

                _context.OrderItems.AddRange(orderItems);

                _context.CartHistories.AddRange(cartHistoriesToAdd);

                await _context.SaveChangesAsync();

                await _cartRepo.ClearCartAsync(userId);

                await transaction.CommitAsync();

                orderDetails.OrderItems = orderItems;

                return orderDetails;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<decimal> CalculateCartTotalAsync(Guid userId)
        {
            return await _cartRepo.GetCartTotalAsync(userId);
        }

        public async Task<bool> ValidateCartStockAsync(Guid userId)
        {
            var cartItems = await _cartRepo.GetCartItemsByUserIdAsync(userId);

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Book == null) continue;

                var currentBook = await _bookRepo.GetBookByIdAsync(cartItem.BookId.Value);
                if (currentBook == null || currentBook.StockQuantity < cartItem.Quantity)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
