using ECommerceWebsite.Models.Helping_Classes;

namespace ECommerceWebsite.Middleware
{
    public class UserActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserActivityMiddleware> _logger;

        public UserActivityMiddleware(RequestDelegate next, ILogger<UserActivityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Track user activity for authenticated users
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("UserId")?.Value;
                var userName = context.User.FindFirst("UserName")?.Value;
                var action = $"{context.Request.Method} {context.Request.Path}";

                // Log significant user activities
                if (IsSignificantActivity(context.Request.Path))
                {
                    _logger.LogInformation("User Activity: {UserName} ({UserId}) performed {Action} at {Timestamp}",
                        userName, userId, action, DateTime.UtcNow);
                }

                // You could also store this in the database for audit trails
                // await _activityService.LogUserActivityAsync(userId, action, context.Connection.RemoteIpAddress?.ToString());
            }

            await _next(context);
        }

        private static bool IsSignificantActivity(PathString path)
        {
            var significantPaths = new[]
            {
                "/Login", "/Logout", "/SignUp",
                "/Cart/AddToCart", "/Cart/PlaceOrder",
                "/Admin", "/Cart/Checkout",
                "/Admin", "/Admin/UpdateOrderStatus"
            };

            return significantPaths.Any(significantPath => path.StartsWithSegments(significantPath));
        }
    }
}