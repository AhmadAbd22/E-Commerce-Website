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
            public static DateTime DateTimeNow()
            {
                return DateTime.UtcNow;
            }

            #region directory creation and file upload
            public static void CreateBookDirectory(Guid? bookId)
            {
                var rootDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var bookDir = rootDir + "/BookImages" + bookId.ToString() + "/";

                if (!Directory.Exists(bookDir))
                    Directory.CreateDirectory(bookDir);
            }

            public static bool IsValidImageExtension(string fileName)
            {
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                return allowedExtensions.Contains(ext);
            }

            public static string GetFilePathForSave(string? bookId, string? filePath = "")
            {
                var rootDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var baseUri = rootDir + $"/BookImages{bookId}/";

                if (string.IsNullOrEmpty(filePath))
                {
                    return baseUri;
                }
                return baseUri + filePath;

            }

            public static async Task<bool> SaveFile(IFormFile file, string filePath)
            {
                try
                {
                    var stream = new MemoryStream();
                    await file.CopyToAsync(stream);
                    var bytes = stream.ToArray();
                    stream.Close();

                    await File.WriteAllBytesAsync(filePath, bytes);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            public static async Task<bool> DeleteFile(string userId, string filePath)
            {
                try
                {
                    var fileDir = GetFilePathForSave(userId.ToString());
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        string[] getName = filePath.Split("/");

                        // Check if a book image already exists for the user
                        string existingBookImagePath = Path.Combine(fileDir, getName.Last());
                        if (System.IO.File.Exists(existingBookImagePath))
                        {
                            System.IO.File.Delete(existingBookImagePath);
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            #endregion

        }
}
