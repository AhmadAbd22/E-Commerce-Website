using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Repository;
using System;
using System.Threading.Tasks;
using ECommerceWebsite.Models.Repository;

namespace ECommerceWebsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly IBookRepository _bookRepo;
        private readonly IAuthorRepository _authorRepo;
        private readonly ICategoryRepository _categoryRepo;

        public AdminController(IBookRepository bookRepo, IAuthorRepository authorRepo, ICategoryRepository categoryRepo)
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _categoryRepo = categoryRepo;
        }

        // GET: /Admin
        public async Task<IActionResult> Admin()
        {
            var books = await _bookRepo.GetAllBooksAsync();
            return View(books);
        }

        // GET: /Admin/DeletedBooks
        public async Task<IActionResult> DeletedBooks()
        {
            var deletedBooks = await _bookRepo.GetDeletedBooksAsync();
            return View(deletedBooks);
        }

        // GET: e
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return RedirectToAction("Admin");
        }

        // POST:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return RedirectToAction("Admin", dto);
            }

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.Stock,
                AuthorId = dto.AuthorId,
                CategoryId = dto.CategoryId,
                ImageUrl = dto.ImageUrl,
                isActive = true,
                CreatedAt = DateTime.UtcNow,
                PublicationDate = dto.PublicationDate
            };

            await _bookRepo.AddBookAsync(book);
            return RedirectToAction("Index");
        }

        // GEt
        public async Task<IActionResult> Edit(Guid id)
        {
            var book = await _bookRepo.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            var dto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Price = book.Price,
                Stock = book.StockQuantity,
                AuthorId = book.AuthorId,
                CategoryId = book.CategoryId,
                ImageUrl = book.ImageUrl,
                PublicationDate = book.PublicationDate
            };

            await LoadDropdowns();
            return View(dto);
        }

        // POST: 
        [HttpPost]
        public async Task<IActionResult> Edit(BookDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(dto);
            }

            var book = await _bookRepo.GetBookByIdAsync(dto.Id);
            if (book == null)
                return NotFound();

            book.Title = dto.Title;
            book.Description = dto.Description;
            book.Price = dto.Price;
            book.StockQuantity = dto.Stock;
            book.AuthorId = dto.AuthorId;
            book.CategoryId = dto.CategoryId;
            book.ImageUrl = dto.ImageUrl;
            book.PublicationDate = dto.PublicationDate;

            await _bookRepo.UpdateBookAsync(book);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _bookRepo.DeleteBookAsync(id);
            return RedirectToAction("Index");
        }

        // GET
        public async Task<IActionResult> Search(string term)
        {
            var books = await _bookRepo.SearchActiveBooksAsync(term);
            return View("Index", books);
        }

        // GET
        public async Task<IActionResult> FilterByAuthor(Guid authorId)
        {
            var books = await _bookRepo.GetBooksByAuthorAsync(authorId.ToString());
            return View("Index", books);
        }

        // GET
        public async Task<IActionResult> FilterByCategory(Guid categoryId)
        {
            var books = await _bookRepo.GetBooksByCategoryAsync(categoryId.ToString());
            return View("Index", books);
        }

        private async Task LoadDropdowns()
        {
            var authors = await _authorRepo.GetAllAuthorsAsync();
            var categories = await _categoryRepo.GetAllCategoriesAsync();

            ViewBag.Authors = new SelectList(authors, "Id", "AuthorName");
            ViewBag.Categories = new SelectList(categories, "Id", "CategoryType");
        }
    }
}
