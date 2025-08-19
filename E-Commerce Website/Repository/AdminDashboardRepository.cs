using ECommerceWebsite.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebsite.Repository
{
    public interface IAdminDashboardRepository
    {
        //Total sales
        Task<int> GetTotalSales();

        //total orders
        Task<int> GetTotalOrders();

        //pending orders
        Task<int> GetPendingOrders();

        // monthly basis sales
        Task<decimal> GetMonthlySales();

        //weekly bais sales
        Task<decimal> GetWeeklySales();

        // Total users
        Task<int> GetTotalUsers();
    }
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly ECommerceWebsiteDbContext _context;

        public AdminDashboardRepository(ECommerceWebsiteDbContext context)
        {
            _context = context;
        }

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
                                .SumAsync(o => o.TotalAmount);
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

        public async Task<int> GetTotalUsers()
        {
            return await _context.Users.CountAsync();
        }


    }
}
