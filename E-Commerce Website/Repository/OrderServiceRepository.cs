using ECommerceWebsite.Models;
using ECommerceWebsite.Repository;

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

        public OrderServiceRepository(
            ICartRepository cartRepo,
            IOrderRepository orderRepo,
            ICartHistoryRepository cartHistoryRepo,
            IBookRepository bookRepo)
        {
            _cartRepo = cartRepo;
            _orderRepo = orderRepo;
            _cartHistoryRepo = cartHistoryRepo;
            _bookRepo = bookRepo;
        }

        public async Task<OrderDetails> PlaceOrderFromCartAsync(Guid userId, string shippingAddress, string city, string phoneNumber)
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
                UpdatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            // 5. Create Order Items and Cart History
            foreach (var cartItem in cartItems)
            {
                if (cartItem.Book == null) continue;

                // Create Order Item
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    BookId = cartItem.BookId,
                    UserId = userId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Book.Price,
                    Orderetails = orderDetails,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                orderDetails.OrderItems.Add(orderItem);

                // Create Cart History entry
                var cartHistory = new CartHistory
                {
                    UserId = userId,
                    BookId = cartItem.BookId,
                    OrderItemId = orderItem.Id,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchase = cartItem.Book.Price
                };

                await _cartHistoryRepo.AddCartHistoryAsync(cartHistory);

                // Update book stock
                var book = await _bookRepo.GetBookByIdAsync(cartItem.BookId.Value);
                if (book != null)
                {
                    book.StockQuantity -= cartItem.Quantity;
                    await _bookRepo.UpdateBookAsync(book);
                }
            }

            // 6. Save Order
            await _orderRepo.PlaceOrder(orderDetails);

            // 7. Clear Cart
            await _cartRepo.ClearCartAsync(userId);

            return orderDetails;
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
