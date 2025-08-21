using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Models.Enums;
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
        Task<PagedResult<OrderDetails>> GetAllOrdersPaged(int pageNumber, int pageSize, string? status, string? sortBy);

        Task<PagedResult<OrderDetails>> GetOrderPlaced(int pageNumber, int pageSize);

        Task <string?> GetOrderStatus(Guid orderId, Guid userId);
        Task<string?> UpdateOrderStatus(Guid orderId, Guid userId, string orderStatus);
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
            .ThenInclude(oi => oi.Book)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<OrderDetails>> GetOrderByUserIdAsync(Guid userId)
        {
            return await _context.OrderDetails
                     .Where(o => o.UserId == userId)
                     .Include(o => o.OrderItems)
                     .ThenInclude(oi => oi.Book)
                        .ThenInclude(b => b.Author)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
        }

        public async Task PlaceOrder(OrderDetails order)
        {
            await _context.OrderDetails.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(OrderDetails order)
        {
            var existingOrder = await GetOrderByIdAsync(order.Id);
            if (existingOrder != null)
            {
                existingOrder.OrderStatus = order.OrderStatus;
                existingOrder.UpdatedAt = order.UpdatedAt;
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

        public async Task<PagedResult<OrderDetails>> GetAllOrdersPaged(int pageNumber, int pageSize, string? status, string? sortBy)
        {
            var query = _context.OrderDetails
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            // sorting
            switch (sortBy)
            {
                case "date_asc":
                    query = query.OrderBy(o => o.OrderDate);
                    break;
                case "total_desc":
                    query = query.OrderByDescending(o => o.TotalAmount);
                    break;
                case "total_asc":
                    query = query.OrderBy(o => o.TotalAmount);
                    break;
                default: // "date_desc" or any other value
                    query = query.OrderByDescending(o => o.OrderDate);
                    break;
            }

            // Pagination]
            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<OrderDetails>
            {
                Items = orders,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
            };
        }
        public async Task<PagedResult<OrderDetails>> GetOrderPlaced(int pageNumber, int pageSize)
        {
            var query = _context.OrderDetails
                             .Include(o => o.User)
                             .Include(od => od.OrderStatus)
                             .OrderByDescending(od => od.OrderDate);

            var totalCount = await query.CountAsync();

            var placedOrders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(od => new OrderDetails
                {
                    Id = od.Id,
                    TotalAmount = od.TotalAmount,
                    OrderDate = od.OrderDate,
                    OrderStatus = od.OrderStatus,
                    ShippingAddress = od.ShippingAddress,
                    City = od.City,
                    PhoneNumber = od.PhoneNumber,
                    UserId = od.UserId,
                    User = od.User == null ? null : new User
                    {
                        FirstName = od.User.FirstName,
                        LastName = od.User.LastName,
                        Address = od.User.Address,
                        City = od.User.City,
                    },
                })
                .ToListAsync();

            return new PagedResult<OrderDetails>
            {
                Items = placedOrders,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
            };

        }

        public async Task<string?> GetOrderStatus(Guid orderId, Guid userId)
        {
            return await _context.OrderDetails
                               .Where(o => o.Id == orderId && o.UserId == userId)
                               .Select(o => o.OrderStatus)
                               .FirstOrDefaultAsync();
        }


        /// <summary>
        /// This method updates the order status. When the user cancels the order, 
        /// the string is passed to the method in controller that further passes down the string to the method below.
        /// From (ADMIN VIEW) the admin will be allowed to update the statuses from "pending to onwward i.e Confirmed, Shipped, Delivered or can
        /// cancel. Once cancelled the statuses cannot be updated (this logic shuold be handled in the controller)
        /// </summary>

        public Task<string?> UpdateOrderStatus(Guid orderId, Guid userId, string orderStatus)
        {
            // get status of the order
            var getStatus = _context.OrderDetails
                 .Where(o => o.Id == orderId && o.UserId == userId)
                 .Select(o => o.OrderStatus)
                 .FirstOrDefaultAsync();

            var order = new OrderDetails
            {
                Id = orderId,
                UserId = userId,
                OrderStatus = orderStatus
            };
            _context.OrderDetails.Update(order);
            _context.SaveChangesAsync();
            return getStatus;
        }

        #endregion
    }
}