using ECommerceWebsite.Models.Dtos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ECommerceWebsite.Models.Enums
{
    public class GeneralPurpose
    {
        private readonly HttpContext hcontext;
        public GeneralPurpose(IHttpContextAccessor haccess)
        {
            hcontext = haccess.HttpContext;
        }

        public static DateTime DateTimeNow()
        {
            return DateTime.UtcNow;
        }

        #region Book Image Operations - CLEANED UP VERSION
        public static void CreateBookDirectory(Guid bookId)
        {
            var rootDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var bookDir = Path.Combine(rootDir, "BookImages", $"book-{bookId}");
            if (!Directory.Exists(bookDir))
                Directory.CreateDirectory(bookDir);
        }

        public static string GetBookImagePathForSave(Guid bookId, string fileName)
        {
            var rootDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var bookDir = Path.Combine(rootDir, "BookImages", $"book-{bookId}");
            return Path.Combine(bookDir, fileName);
        }

        public static string GetBookImageUrl(Guid bookId, string fileName)
        {
            return $"/BookImages/book-{bookId}/{fileName}";
        }

        public static bool IsValidImageExtension(string fileName)
        {
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(ext);
        }

        public static bool IsValidImageSize(IFormFile file, int maxSizeInMB = 2)
        {
            var maxSizeInBytes = maxSizeInMB * 1024 * 1024;
            return file.Length <= maxSizeInBytes;
        }

        public static async Task<bool> SaveFile(IFormFile file, string filePath)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> DeleteBookImage(Guid bookId, string fileName)
        {
            try
            {
                var filePath = GetBookImagePathForSave(bookId, fileName);
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion

    }
}
