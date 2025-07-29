using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebsite.Repository
{
    public interface IOrderRepository
    {
        //Place Order
        Task PlaceOrder(OrderDetails order);

        //OrderByIdAsync
        Task<OrderDetails?> GetOrderByIdAsync(Guid orderId);
        //OrderByUserIdAsync
        Task<List<OrderDetails>> GetOrderByUserIdAsync(Guid userId);
        //Update Order
        Task UpdateOrderAsync(OrderDetails order);

    }
    public class OrderRepository : IOrderRepository
    {
        private readonly ECommerceWebsiteDbContext _context;
        public OrderRepository(ECommerceWebsiteDbContext context)
        {
            _context = context;
        }
        public async Task<OrderDetails?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.OrderDetails
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<OrderDetails>> GetOrderByUserIdAsync(Guid userId)
        {
           var order = await _context.OrderDetails
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ToListAsync();
            if (order == null || !order.Any())
            {
                throw new KeyNotFoundException("No orders found for the specified user.");
            }
            return order;
        }

        public async Task PlaceOrder(OrderDetails order)
        {
            await _context.OrderDetails.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(OrderDetails order)
        {
            await _context.AddRangeAsync(order.OrderItems);
            var existingOrder = await GetOrderByIdAsync(order.Id);
            if (existingOrder == null)
            {
                _context.OrderDetails.Update(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}