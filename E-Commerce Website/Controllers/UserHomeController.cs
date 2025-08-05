using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Repository;
using ECommerceWebsite.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebsite.Controllers
{
    public class UserHomeController : Controller
    {
        private readonly IBookRepository _bookRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IAuthorRepository _authorRepo;

        public UserHomeController(IBookRepository bookRepo, ICategoryRepository categoryRepo, IAuthorRepository authorRepo)
        {
            _bookRepo = bookRepo;
            _categoryRepo = categoryRepo;
            _authorRepo = authorRepo;
        }

        public async Task<IActionResult> UserHome()
        {
            try
            {
                var books = await _bookRepo.GetActiveBooksAsync();
                var authors = await _authorRepo.GetAllAuthorsAsync();
                var categories = await _categoryRepo.GetAllCategoriesAsync();

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
                    books = await _bookRepo.FilterBooksAsync(authorId.Value, minPrice, maxPrice);
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





    }
}