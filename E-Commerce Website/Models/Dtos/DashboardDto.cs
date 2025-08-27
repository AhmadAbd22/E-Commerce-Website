using System.Text.Json;
namespace ECommerceWebsite.Models.Dtos
{
    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Values { get; set; } = new();
        public List<WeeklySalesChartDto> WeeklySalesByWeekChart { get; set; }
    }

    public class StatDto
    {
        public string Title { get; set; } = string.Empty;   // "Total Sales"
        public decimal Value { get; set; }                  // 25000   
        public string Unit { get; set; } = string.Empty;    // "$" or "Orders"
    }

    public class OrderSummaryDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }

    public class CustomerStatDto
    {
        public int TotalCustomers { get; set; }             // 5000
        public int ActiveCustomers { get; set; }             // 4500
        public int NewCustomersThisMonth { get; set; }       // 200 
        public decimal AverageOrderValue { get; set; }       // Rs. 1500
    }

    public class DailySalesDto
    {
        public DateTime Date { get; set; }                //"2023-10-01"
        public decimal TotalSales { get; set; }            //Rs. 5000
        public int OrderCount { get; set; }                //150
        public List<ProductDto> TopProducts { get; set; } = new(); // List of top products sold that day
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int QuantitySold { get; set; }
    }
    public class DashboardDto
    {
        // KPIs
        public decimal TotalSales { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ActiveCustomers { get; set; }
        public int CancelledOrders { get; set; }

        public List<StatDto> Stats { get; set; } = new();

        // Charts
        public ChartDataDto WeeklySalesByDay { get; set; } = new();
        public ChartDataDto WeeklySalesByWeek { get; set; } = new();
    }



public class WeeklySalesDto
    {
        public int Year { get; set; }
        public int WeekNumber { get; set; }   
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public int OrdersCount { get; set; }
    }

    public class WeeklySalesChartDto   
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> SalesValues { get; set; } = new();
    }

    public class MonthlySalesDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class YearlySalesDto
    {
        public int Year { get; set; }
        public decimal TotalSales { get; set; }
    }


    // This represents a single product line item within an order.
    public class AdminOrderItemDto
    {
        public string BookTitle { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice; // Calculated property
    }

    // This represents a single, complete order for the admin view.
    public class AdminOrderViewDto
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }

        //user's details
        public string UserFullName { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }

        // Aggregated details about the items in the order
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }

        // A list of all individual items within this order
        public List<AdminOrderItemDto> OrderItems { get; set; } = new List<AdminOrderItemDto>();
    }
}