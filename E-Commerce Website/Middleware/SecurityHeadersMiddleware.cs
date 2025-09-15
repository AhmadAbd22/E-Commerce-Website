namespace ECommerceWebsite.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            var headers = context.Response.Headers;

            // Prevent clickjacking
            headers.Add("X-Frame-Options", "DENY");

            // Enable XSS filtering
            headers.Add("X-XSS-Protection", "1; mode=block");

            // Prevent MIME type sniffing
            headers.Add("X-Content-Type-Options", "nosniff");

            // Referrer policy
            headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content Security Policy
            headers.Add("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; " +
                "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self'");

            // Strict Transport Security (for HTTPS)
            if (context.Request.IsHttps)
            {
                headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }

            await _next(context);
        }
    }
}