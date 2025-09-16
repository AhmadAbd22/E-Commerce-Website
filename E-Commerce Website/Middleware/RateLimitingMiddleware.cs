using ECommerceWebsite.Models.Helping_Classes;
using System.Collections.Concurrent;
using System.Net;

namespace ECommerceWebsite.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;
        private static readonly ConcurrentDictionary<string, ClientRequestTracker> _clients = new();

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (RequestPathHelper.ShouldSkipLogging(context.Request.Path))
            {
                await _next(context);
                return;
            }
            var clientId = GetClientIdentifier(context);
            var endpoint = $"{context.Request.Method}:{context.Request.Path}";

            // Different limits for different endpoints
            var (requestLimit, timeWindow) = GetLimitsForEndpoint(endpoint);

            var tracker = _clients.GetOrAdd(clientId, _ => new ClientRequestTracker());

            if (!tracker.CanMakeRequest(requestLimit, timeWindow))
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", clientId, endpoint);
                
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Add("Retry-After", timeWindow.TotalSeconds.ToString());
                
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            tracker.RecordRequest();
            await _next(context);
        }

        private static string GetClientIdentifier(HttpContext context)
        {
            // Use user ID if authenticated, otherwise IP address
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                    return $"User:{userId}";
            }

            return $"IP:{context.Connection.RemoteIpAddress}";
        }

        private (int requestLimit, TimeSpan timeWindow) GetLimitsForEndpoint(string endpoint)
        {
            // More generous limits for development
            if (_environment.IsDevelopment())
            {
                if (endpoint.Contains("/Login") || endpoint.Contains("/SignUp"))
                    return (50, TimeSpan.FromMinutes(1)); // 50 requests per minute in dev

                if (endpoint.Contains("/Cart/Add") || endpoint.Contains("/Cart/PlaceOrder"))
                    return (100, TimeSpan.FromMinutes(1)); // 100 requests per minute in dev

                return (1000, TimeSpan.FromMinutes(1)); // 1000 requests per minute in dev
            }

            // Stricter limits for production
            if (endpoint.Contains("/Login") || endpoint.Contains("/SignUp"))
                return (15, TimeSpan.FromMinutes(1)); // 15 requests per minute

            if (endpoint.Contains("/Cart/Add") || endpoint.Contains("/Cart/PlaceOrder"))
                return (15, TimeSpan.FromMinutes(1)); // 15 requests per minute

            // Default limits
            return (100, TimeSpan.FromMinutes(1)); // 100 requests per minute
        }

        private class ClientRequestTracker
        {
            private readonly List<DateTime> _requestTimes = new();
            private readonly object _lock = new();

            public bool CanMakeRequest(int limit, TimeSpan timeWindow)
            {
                lock (_lock)
                {
                    var cutoff = DateTime.UtcNow - timeWindow;
                    _requestTimes.RemoveAll(time => time < cutoff);
                    
                    return _requestTimes.Count < limit;
                }
            }

            public void RecordRequest()
            {
                lock (_lock)
                {
                    _requestTimes.Add(DateTime.UtcNow);
                }
            }
        }

        // Add this static method to the RateLimitingMiddleware class
        public static void ClearRateLimit(string clientId)
        {
            _clients.TryRemove(clientId, out _);
        }

        public static void ClearAllRateLimits()
        {
            _clients.Clear();
        }
    }
}