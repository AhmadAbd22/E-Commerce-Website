using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Repository;
using ECommerceWebsite.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceWebsite.Controllers
{
    [Authorize(Roles = "User")]
    public class UserHomeController : Controller
    {
        private readonly IBookRepository _bookRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IAuthorRepository _authorRepo;
        private readonly ICartRepository _cartRepo;

        public UserHomeController(IBookRepository bookRepo, ICategoryRepository categoryRepo, IAuthorRepository authorRepo, ICartRepository cartRepo)
        {
            _bookRepo = bookRepo;
            _categoryRepo = categoryRepo;
            _authorRepo = authorRepo;
            _cartRepo = cartRepo;
        }

        [AllowAnonymous]
        public async Task<IActionResult> UserHome()
        {
            try
            {
                var books = await _bookRepo.GetActiveBooksAsync();
                var authors = await _authorRepo.GetAllAuthorsAsync();
                var categories = await _categoryRepo.GetAllCategoriesAsync();

                await SetCartItemCount();

                ViewData["Authors"] = authors;
                ViewData["Categories"] = categories;

                var dtos = books.Select(book => new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Price = book.Price,
                    Stock = book.StockQuantity,
                    Author = book.Author,
                    ImageUrl = book.ImageUrl,
                }).ToList();

                return View(dtos);
            }
            catch (Exception ex)
            {
                return View("Error", new { message = "An error occurred while loading the home page." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UserHome(string search, Guid? authorId, Guid? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            IEnumerable<Book> books;
            try
            {
                if (!string.IsNullOrEmpty(search))
                {
                    books = await _bookRepo.SearchActiveBooksAsync(search);
                }
                else if (authorId.HasValue || categoryId.HasValue || minPrice.HasValue || maxPrice.HasValue)
                {
                    books = await _bookRepo.FilterBooksAsync(authorId ?? Guid.Empty, minPrice, maxPrice);
                        if (categoryId.HasValue)
                    {
                        books = books.Where(b => b.CategoryId == categoryId.Value);
                    }
                }
                else
                {
                    books = await _bookRepo.GetActiveBooksAsync();
                }

                var authors = await _authorRepo.GetAllAuthorsAsync();
                var categories = await _categoryRepo.GetAllCategoriesAsync();

                await SetCartItemCount();

                ViewData["Authors"] = authors;
                ViewData["Categories"] = categories;

                var dtos = books.Select(book => new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Price = book.Price,
                    Stock = book.StockQuantity,
                    Author = book.Author,
                    ImageUrl = book.ImageUrl,
                }).ToList();

                return View(dtos);
            }
            catch (Exception)
            {
                return View("Error", new { message = "An error occurred while searching for books." });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var book = await _bookRepo.GetActiveBookByIdAsync(id);
                if (book == null)
                {
                    TempData["Error"] = "Book not found!";
                    return RedirectToAction("UserHome");
                }

                var authors = await _authorRepo.GetAllAuthorsAsync();
                var categories = await _categoryRepo.GetAllCategoriesAsync();
                ViewData["Authors"] = authors;
                ViewData["Categories"] = categories;

                var bookDto = new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Price = book.Price,
                    Stock = book.StockQuantity,
                    Author = book.Author,
                    Category = book.Category,
                    ImageUrl = book.ImageUrl,
                    Description = book.Description,
                    PublicationDate = book.PublicationDate
                };

                return View("ViewProduct", bookDto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the book details.";
                return RedirectToAction("UserHome");
            }
        }



        //helper method

        private Guid GetCurrentUserId()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return Guid.Empty;
                }

                var userIdClaim = User.FindFirst(ClaimTypes.Sid)
                               ?? User.FindFirst("EncId")
                               ?? User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                {
                    return Guid.Empty;
                }

                if (Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return userId;
                }

                return Guid.Empty;
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        // Helper method
        private async Task SetCartItemCount()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetCurrentUserId();
                if (userId != Guid.Empty)
                {
                    var cartItemCount = await _cartRepo.GetCartItemCountAsync(userId);
                    ViewBag.CartItemCount = cartItemCount;
                }
                else
                {
                    ViewBag.CartItemCount = 0;
                }
            }
            else
            {
                ViewBag.CartItemCount = 0;
            }
        }
    }
}