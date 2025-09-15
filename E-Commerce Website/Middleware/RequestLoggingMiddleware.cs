using System.Diagnostics;
using System.Text;

namespace ECommerceWebsite.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            
            // Skip logging for static assets
            if (ShouldSkipLogging(request.Path))
            {
                await _next(context);
                return;
            }
            
            // Log request details
            var requestBody = await GetRequestBodyAsync(request);
            
            _logger.LogInformation("Request: {Method} {Path} {QueryString} - User: {User} - IP: {IP}",
                request.Method,
                request.Path,
                request.QueryString,
                context.User?.Identity?.Name ?? "Anonymous",
                GetClientIpAddress(context));

            // Log request body for POST requests (excluding sensitive endpoints)
            if (request.Method == "POST" && !IsSensitiveEndpoint(request.Path))
            {
                _logger.LogDebug("Request Body: {RequestBody}", requestBody);
            }

            // Continue with the pipeline
            await _next(context);

            stopwatch.Stop();

            // Log response details
            _logger.LogInformation("Response: {StatusCode} in {ElapsedMilliseconds}ms for {Method} {Path}",
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                request.Method,
                request.Path);
        }

        private static async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            if (!request.Body.CanSeek)
            {
                request.EnableBuffering();
            }

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return body;
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "Unknown";
        }

        private static bool IsSensitiveEndpoint(PathString path)
        {
            var sensitiveEndpoints = new[] { "/Login", "/SignUp", "/Admin" };
            return sensitiveEndpoints.Any(endpoint => path.StartsWithSegments(endpoint));
        }

        private static bool ShouldSkipLogging(PathString path)
        {
            var skipPaths = new[] { "/css", "/js", "/images", "/favicon.ico", "/assets" };
            return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath));
        }
    }
}