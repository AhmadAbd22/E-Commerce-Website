namespace ECommerceWebsite.Models.Helping_Classes
{
    public static class RequestPathHelper
    {
        private static readonly string[] skipPaths = { "/css", "/js", "/images", "/favicon.ico", "/assets" };
        public static bool ShouldSkipLogging(PathString path)
        {
            return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath));
        }
    }
}
