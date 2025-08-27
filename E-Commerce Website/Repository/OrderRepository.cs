using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Models.Enums;
using ECommerceWebsite.Models.Helping_Classes;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

        Task<string?> GetOrderStatus(Guid orderId, Guid userId);
        Task<string?> UpdateOrderStatus(Guid orderId, Guid userId, string orderStatus);


        #region Charts Region

        #region total sales, total orders, pending orders, weekly sales, monthly sales
        Task<decimal> GetTotalSales();

        Task<int> GetTotalOrders();


        Task<int> GetTotalPendingOrders();
        Task<int> GetTotalCompletedOrders();
        Task<int> GetTotalCancelledOrders();

        Task<decimal> GetTotalMonthlySales();
        Task<decimal> GetTotalWeeklySales();

        Task<decimal> GetTotalDailySales();
        #endregion


        #region charts by week
        Task<List<DailySalesDto>> GetWeeklySalesByDay(int isoYear, int isoWeek);
        Task<List<WeeklySalesDto>> GetWeeklySalesByWeek();
        #endregion

        #region charts by month

        Task<List<MonthlySalesDto>> GetMonthlySalesByDay();


        #region charts by year

        Task<List<YearlySalesDto>> GetYearlySalesByMonth();
        Task<List<YearlySalesDto>> GetYearlySalesByYear();

        #endregion
        #endregion

        #endregion

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
                existingOrder.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        #region methods for charts

        #region totals
        public async Task<decimal> GetTotalDailySales()   //status should be delivered and sum the total of each order (repeat for all 'Total' functions
        {
            return await _context.OrderDetails
                .Where(o => o.OrderStatus == "Delivered" && o.OrderDate.Date == DateTime.Today)
                .SumAsync(o => o.TotalAmount);
        }
        public async Task<decimal> GetTotalMonthlySales()
        {
            return await _context.OrderDetails
                                 .Where(o => o.OrderStatus == "Delivered" && o.OrderDate >= DateTime.Now.AddMonths(-1))
                                 .SumAsync(o => o.TotalAmount);
        }
        public async Task<decimal> GetTotalWeeklySales()
        {
            return await _context.OrderDetails
                                .Where(o => o.OrderStatus == "Delivered" && o.OrderDate >= DateTime.Now.AddDays(-7))
                                .Select(o => (decimal?)o.TotalAmount)
                                .SumAsync() ?? 0m;
        }


        public async Task<int> GetTotalPendingOrders()
        {
            return await _context.OrderDetails
                                    .CountAsync(o => o.OrderStatus == "Pending");
        }

        public async Task<int> GetTotalOrders()
        {
            return await _context.OrderDetails
                                 .CountAsync();
        }

        public async Task<decimal> GetTotalSales()
        {
            return await _context.OrderDetails
                .Where(o => o.OrderStatus == "Delivered")
                .SumAsync(o => o.TotalAmount);
        }

        #endregion


        #region weekly 

        public async Task<List<DailySalesDto>> GetWeeklySalesByDay(int isoYear, int isoWeek)
        {
            var startOfWeek = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(6);

            var sales = await _context.OrderDetails
                .Where(o => o.OrderStatus == "Delivered"
                         && o.OrderDate >= startOfWeek
                         && o.OrderDate <= endOfWeek)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    TotalSales = g.Sum(x => x.TotalAmount),
                    OrderCount = g.Count()
                })
                .ToListAsync();

            // Fill missing days with 0 sales
            var result = Enumerable.Range(0, 7).Select(i =>
            {
                var day = startOfWeek.AddDays(i).Date;
                var daySales = sales.FirstOrDefault(s => s.Date == day);
                return new DailySalesDto
                {
                    Date = day,
                    TotalSales = daySales?.TotalSales ?? 0,
                    OrderCount = daySales?.OrderCount ?? 0
                };
            }).ToList();

            return result;
        }



        /// <summary>
        /// This method fetches weekly sales data grouped by ISO week number.
        /// ISO week follows the ISO-8601 standard, where weeks start on Monday and week 1 is the week with the first Thursday of the year.
        /// </summary>
        /// <returns></returns>

        public async Task<List<WeeklySalesDto>> GetWeeklySalesByWeek()
        {
            var today = DateTime.Today;

            // Get ISO year & week for the start of the year and today
            int startYear = ISOWeek.GetYear(new DateTime(today.Year, 1, 1));   //2025 , month (jan) , date (1)
            int startWeek = ISOWeek.GetWeekOfYear(new DateTime(today.Year, 1, 1));  //ISO week follows ISO-8601 standard (
            int currentYear = ISOWeek.GetYear(today);
            int currentWeek = ISOWeek.GetWeekOfYear(today);


            //first week start to start from the date when fetching orders.

            var firstWeekStart = ISOWeek.ToDateTime(ISOWeek.GetYear(new DateTime(today.Year, 1, 4)), 1, DayOfWeek.Monday);  //Jan 4 is always in Week 1 

            // The expression will be GetYear (2025,Jan,4   , 1 , DayOfWeek.Monday)  => Monday of ISO week 1), so we'll get 30 Dec, 2024

            // Fetch sales data first

            var orders = await _context.OrderDetails
                .Where(o => o.OrderStatus == "Delivered" 
                        && o.OrderDate >= firstWeekStart   // starting from 30 Dec,2024
                        && o.OrderDate <= today)
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync();

            // Group by ISO year + week
            var groupedSales = orders
                .GroupBy(o =>
                {
                    int isoYear = ISOWeek.GetYear(o.OrderDate);
                    int isoWeek = ISOWeek.GetWeekOfYear(o.OrderDate);
                    return new { isoYear, isoWeek };
                })
                .ToDictionary(
                    g => (Year: g.Key.isoYear, Week: g.Key.isoWeek),
                    g => new { TotalSales = g.Sum(x => x.TotalAmount), OrdersCount = g.Count() }
                );

            // Generate all weeks from start of year to today
            var result = new List<WeeklySalesDto>();
            DateTime cursor = ISOWeek.ToDateTime(startYear, 1, DayOfWeek.Monday);

            while (cursor <= today)
            {
                int isoYear = ISOWeek.GetYear(cursor);
                int isoWeek = ISOWeek.GetWeekOfYear(cursor);
                DateTime startOfWeek = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
                DateTime endOfWeek = startOfWeek.AddDays(6);

                result.Add(new WeeklySalesDto
                {
                    Year = isoYear,
                    WeekNumber = isoWeek,
                    StartDate = startOfWeek,
                    EndDate = endOfWeek,
                    TotalSales = groupedSales.ContainsKey((isoYear, isoWeek))
                                ? groupedSales[(isoYear, isoWeek)].TotalSales
                                : 0,
                    OrdersCount = groupedSales.ContainsKey((isoYear, isoWeek))
                    ? groupedSales[(isoYear, isoWeek)].OrdersCount
                    : 0
                });

                cursor = cursor.AddDays(7); // jump to next week
            }

            return result.OrderBy(r => r.Year).ThenBy(r => r.WeekNumber).ToList();
        }



        #endregion
        public Task<List<MonthlySalesDto>> GetMonthlySalesByDay()
        {
            throw new NotImplementedException();
        }

        public Task<List<YearlySalesDto>> GetYearlySalesByMonth()
        {
            throw new NotImplementedException();
        }

        public Task<List<YearlySalesDto>> GetYearlySalesByYear()
        {
            throw new NotImplementedException();
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

        public async Task<int> GetTotalCompletedOrders()
        {
            return await _context.OrderDetails
                                     .CountAsync(o => o.OrderStatus == "Delivered");
        }

        public async Task<int> GetTotalCancelledOrders()
        {
            return await _context.OrderDetails
                                     .CountAsync(o => o.OrderStatus == "Cancelled");
        }



        #endregion
    }
}