using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Repository;
using ECommerceWebsite.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ECommerceWebsite.Models.Helping_Classes;

namespace ECommerceWebsite.Controllers
{
    [Authorize(Roles = "User")]
    public class UserHomeController : Controller
    {
        private readonly IBookRepository _bookRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IAuthorRepository _authorRepo;
        private readonly ICartRepository _cartRepo;
        private readonly Authorization _authorization;

        public UserHomeController(IBookRepository bookRepo, ICategoryRepository categoryRepo, IAuthorRepository authorRepo, ICartRepository cartRepo, Authorization authorization)
        {
            _bookRepo = bookRepo;
            _categoryRepo = categoryRepo;
            _authorRepo = authorRepo;
            _cartRepo = cartRepo;
            _authorization = authorization;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> UserHome(string search, Guid? authorId, Guid? categoryId,
    decimal? minPrice, decimal? maxPrice, string sortBy, string sortOrder,
    string viewType = "grid", int pageNumber = 1)
        {
            const int pageSize = 12;
            PagedResult<Book> pagedBooks;

            if (!string.IsNullOrWhiteSpace(search))
            {
                pagedBooks = await _bookRepo.SearchActiveBooksPagedAsync(search.Trim(), pageNumber, pageSize);
            }
            else
            {
                // Combined Filter + Sort
                pagedBooks = await _bookRepo.FilterAndSortBooksPagedAsync(
                    authorId, categoryId, minPrice, maxPrice,
                    sortBy, sortOrder, pageNumber, pageSize);
            }

            var pagedDto = new PagedResult<BookDto>
            {
                CurrentPage = pagedBooks.CurrentPage,
                PageSize = pagedBooks.PageSize,
                TotalCount = pagedBooks.TotalCount,
                Items = pagedBooks.Items.Select(book => new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    Price = book.Price,
                    Stock = book.StockQuantity,
                    AuthorId = book.AuthorId,
                    CategoryId = book.CategoryId,
                    PublicationDate = book.PublicationDate,
                    Author = book.Author,
                    Category = book.Category,
                    ImageUrl = book.ImageUrl
                }).ToList()
            };

            await PopulateViewDataForUser();
            await SetCartItemCount();

            // Preserve all filter/sort/view state
            ViewData["Search"] = search;
            ViewData["AuthorId"] = authorId;
            ViewData["CategoryId"] = categoryId;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserBookGridPartial", pagedDto);
            }

            return View(pagedDto);
        }

        [HttpPost]
        public IActionResult UserHome(string search, Guid? authorId, Guid? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            return RedirectToAction("UserHome", new { search, authorId, categoryId, minPrice, maxPrice });
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

        // Helper method
        private async Task SetCartItemCount()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _authorization.GetCurrentUserId();
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

        private async Task PopulateViewDataForUser()
        {
            ViewData["Authors"] = await _authorRepo.GetAllAuthorsAsync();
            ViewData["Categories"] = await _categoryRepo.GetAllCategoriesAsync();
        }
    }
}