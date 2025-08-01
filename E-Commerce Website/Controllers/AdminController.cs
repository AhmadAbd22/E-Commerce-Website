using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Repository;
using System;
using System.Threading.Tasks;
using ECommerceWebsite.Models.Repository;
using ECommerceWebsite.Models.Enums;
using System.Net;

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
            var dtos = books.Select(book => new BookDto
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
                Category = book.Category
            }).ToList();

            return View(dtos);
        }

        // GET
        public async Task<IActionResult> DeletedBooks()
        {
            var deletedBooks = await _bookRepo.GetDeletedBooksAsync();
            return View(deletedBooks);
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            var authors = await _authorRepo.GetAllAuthorsAsync(); 
            var dto = new BookDto
            {
                CategoriesList = categories.ToList(),
                AuthorsList = authors.ToList() 
            };
            return View(dto);
        }


        [HttpPost]
        public async Task<IActionResult> Create(BookDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) ||
                dto.Price <= 0 ||
                dto.Stock < 0 ||
                dto.ImageFile == null || dto.ImageFile.Length == 0 ||
                dto.PublicationDate == null)
            {
                ViewData["Message"] = "All fields are required and must be valid.";

                dto.CategoriesList = (await _categoryRepo.GetAllCategoriesAsync()).ToList();
                dto.AuthorsList = (await _authorRepo.GetAllAuthorsAsync()).ToList();
                return View(dto);
            }

            var bookId = Guid.NewGuid();
            GeneralPurpose.CreateBookDirectory(bookId);

            var fileName = string.IsNullOrWhiteSpace(dto.FileName)
                ? Path.GetFileName(dto.ImageFile.FileName)
                : dto.FileName + Path.GetExtension(dto.ImageFile.FileName);

            var filePath = GeneralPurpose.GetBookImagePathForSave(bookId, fileName);
            var imageSaved = await GeneralPurpose.SaveFile(dto.ImageFile, filePath);

            if (!imageSaved)
            {
                ModelState.AddModelError("ImageFile", "Failed to save image.");
                dto.CategoriesList = (await _categoryRepo.GetAllCategoriesAsync()).ToList();
                dto.AuthorsList = (await _authorRepo.GetAllAuthorsAsync()).ToList();
                return View(dto);
            }

            var imageUrl = GeneralPurpose.GetBookImageUrl(bookId, fileName);


            var book = new Book
            {
                Id = bookId,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.Stock,
                AuthorId = dto.AuthorId,
                CategoryId = dto.CategoryId,
                ImageUrl = imageUrl,
                isActive = (int)enumStatus.Active,
                CreatedAt = DateTime.UtcNow,
                PublicationDate = dto.PublicationDate,
                Path = filePath,
                FileName = fileName // dont use dto since dto is null and the filename is set after the dto is filled
            };

            await _bookRepo.AddBookAsync(book);
            return RedirectToAction("Admin");
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
                PublicationDate = book.PublicationDate,
                FileName = book.FileName
            };

            return View(dto);
        }

        // POST: 
        [HttpPost]
        public async Task<IActionResult> Edit(BookDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var book = await _bookRepo.GetBookByIdAsync(dto.Id);
            if (book == null)
                return NotFound();
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(dto.ImageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Only JPG and PNG images are allowed.");
                    return View(dto);
                }
                if (dto.ImageFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "File size must be less than 2MB.");
                    return View(dto);
                }

                var fileName = string.IsNullOrWhiteSpace(dto.FileName)
                    ? Path.GetFileName(dto.ImageFile.FileName)
                    : dto.FileName + Path.GetExtension(dto.ImageFile.FileName);

                var filePath = GeneralPurpose.GetBookImagePathForSave(book.Id, fileName);
                var imageSaved = await GeneralPurpose.SaveFile(dto.ImageFile, filePath);

                if (!imageSaved)
                {
                    ModelState.AddModelError("ImageFile", "Failed to save image.");
                    return View(dto);
                }

                // Update the image URL
                book.ImageUrl = GeneralPurpose.GetBookImageUrl(book.Id, fileName);
            }

            book.Title = dto.Title;
            book.Description = dto.Description;
            book.Price = dto.Price;
            book.StockQuantity = dto.Stock;
            book.AuthorId = dto.AuthorId;
            book.CategoryId = dto.CategoryId;
            book.PublicationDate = dto.PublicationDate;

            await _bookRepo.UpdateBookAsync(book);
            return RedirectToAction("Admin");
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
            return View("Admin", books);
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
        
        [HttpGet]
        public async Task<IActionResult> AddAuthor()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAuthor(AuthorDto authDto)
        {
            if (string.IsNullOrWhiteSpace(authDto.Name))
            {
                ViewData["EmptyFields"] = "Author name cannot be empty!";
                return View(authDto);
            }

            var authors = await _authorRepo.GetAllAuthorsAsync();
            if (authors.Any(a => a.AuthorName.Equals(authDto.Name.Trim())))
            {
                ViewData["AuthorExists"] = "Author already exists!";
                return View(authDto);
            }

            var author = new Author
            {
                Id = Guid.NewGuid(),
                AuthorName = authDto.Name.Trim()
            };

            await _authorRepo.AddAuthorAsync(author);
            ViewData["Message"] = "Author added successfully!";
            return RedirectToAction("Admin");
        }

        [HttpGet]
        public async Task<IActionResult> AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryDto catDto)
        {
            if (catDto == null || string.IsNullOrWhiteSpace(catDto.CategoryType))
            {
                ViewData["Message"] = "Invalid category data!";
                return View(catDto);
            }

            var categories = await _categoryRepo.GetAllCategoriesAsync();
            if (categories.Any(c => c.CategoryType.Equals(catDto.CategoryType.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ViewData["Existing"] = "Category Already Exists";
                return View(catDto);
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                CategoryType = catDto.CategoryType.Trim(),
            };

            await _categoryRepo.AddCategoryAsync(category);
            ViewData["Message"] = "Category added successfully!";
            return RedirectToAction("Admin", "Admin");
        }

        public async Task RemoveCategory(CategoryDto categoryDto)
        {
            if (categoryDto == null || categoryDto.Id == Guid.Empty)
            {
                ViewData["Message"] = "Invalid category data!";
                return;
            }
            var category = await _categoryRepo.GetCategoryByIdAsync(categoryDto.Id);
            if (category == null)
            {
                ViewData["Message"] = "Category not found!";
                return;
            }
            await _categoryRepo.DeleteCategoryAsync(categoryDto.Id);
            ViewData["Message"] = "Category deleted successfully!";
        }
    }
}
