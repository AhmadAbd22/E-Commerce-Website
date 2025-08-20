using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Models.Helping_Classes;
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

        Task<PagedResult<OrderDetails>> GetAllOrdersPagedAsync(int pageNumber, int pageSize);

        Task<int> GetTotalSales();

        //total orders
        Task<int> GetTotalOrders();

        //pending orders
        Task<int> GetPendingOrders();

        // monthly basis sales
        Task<decimal> GetMonthlySales();

        //weekly basis sales
        Task<decimal> GetWeeklySales();
        Task<List<DailySalesDto>> GetWeeklySalesPerDay();
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
            return await _context.OrderDetails
                 .Where(o => o.UserId == userId)
                 .Include(o => o.OrderItems)
                 .ToListAsync();
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

        #region methods for charts
        public async Task<decimal> GetMonthlySales()
        {
            return await _context.OrderDetails
                                 .Where(o => o.OrderStatus == "Completed" && o.OrderDate >= DateTime.Now.AddMonths(-1))
                                 .SumAsync(o => o.TotalAmount);
        }
        public async Task<decimal> GetWeeklySales()
        {
            return await _context.OrderDetails
                                .Where(o => o.OrderStatus == "Completed" && o.OrderDate >= DateTime.Now.AddDays(-7))
                                .Select(o => (decimal?)o.TotalAmount)
                                .SumAsync() ?? 0m;
        }

        public async Task<List<DailySalesDto>> GetWeeklySalesPerDay()
        {
            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-6);

            return await _context.OrderDetails
                .Where(o => o.OrderStatus == "Completed" &&
                            o.OrderDate.Date >= sevenDaysAgo &&
                            o.OrderDate.Date <= today)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<int> GetPendingOrders()
        {
            return await _context.OrderDetails
                                    .Where(o => o.OrderStatus == "Pending")
                                    .CountAsync();
        }

        public async Task<int> GetTotalOrders()
        {
            return await _context.OrderDetails
                                 .CountAsync();
        }

        public async Task<int> GetTotalSales()
        {
            return await _context.OrderDetails
                .Where(o => o.OrderStatus == "Completed")
                .SumAsync(o => (int)o.TotalAmount);
        }

        #endregion

        #region admin view order

        public async Task<PagedResult<OrderDetails>> GetAllOrdersPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.OrderDetails
                .Include(o => o.User) // Include the user data
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            var totalCount = await query.CountAsync();


            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            foreach (var order in items)
            {
                order.OrderItems = await _context.OrderItems
                    .Include(oi => oi.Book)
                    .Where(oi => oi.OrderDetailsId == order.Id)
                    .ToListAsync();
            }

            return new PagedResult<OrderDetails>
            {
                Items = items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        #endregion
    }
}