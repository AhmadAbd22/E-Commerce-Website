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
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace ECommerceWebsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly IBookRepository _bookRepo;
        private readonly IAuthorRepository _authorRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly ICartRepository _cartRepo;

        public AdminController(IBookRepository bookRepo, IAuthorRepository authorRepo, ICategoryRepository categoryRepo, ICartRepository cartRepo)
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _categoryRepo = categoryRepo;
            _cartRepo = cartRepo;
        }


        // GET: /Admin
        public async Task<IActionResult> Admin()
        {
            var books = await _bookRepo.GetActiveBooksAsync();

            await PopulateViewDataForAdmin();
            await SetCartItemCount();
            ViewData["IsDeletedView"] = false;
            ViewData["Login"] = true;
            
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
                Category = book.Category,
                ImageUrl = book.ImageUrl
            }).ToList();

            return View(dtos);
        }

        // Add this method to your AdminController class

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var book = await _bookRepo.GetBookByIdAsync(id);
                if (book == null)
                {
                    TempData["Error"] = "Book not found!";
                    return RedirectToAction("Admin");
                }

                await PopulateViewDataForAdmin();
                await SetCartItemCount();

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
                    PublicationDate = book.PublicationDate,
                    AuthorId = book.AuthorId,
                    CategoryId = book.CategoryId
                };

                return View("AdminViewProduct", bookDto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the book details.";
                return RedirectToAction("Admin");
            }
        }




        // GET
        public async Task<IActionResult> DeletedBooks()
        {
            var deletedBooks = await _bookRepo.GetDeletedBooksAsync();

            await PopulateViewDataForAdmin();
            await SetCartItemCount();
            ViewData["IsDeletedView"] = true;

            var dtos = deletedBooks.Select(book => new BookDto
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
            }).ToList();

            return View(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreBook(Guid id)
        {
            try
            {
                await _bookRepo.RestoreBookAsync(id);
                TempData["Success"] = "Book restored successfully!";
                return RedirectToAction("DeletedBooks");
            }
            catch (Exception)
            {
                TempData["Error"] = "Error restoring book.";
                return RedirectToAction("DeletedBooks");
            }
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            var authors = await _authorRepo.GetAllAuthorsAsync();
            await SetCartItemCount();

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

            await PopulateViewDataForAdmin();

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
            return RedirectToAction("Admin");
        }

        // GET
        public async Task<IActionResult> Search(string term)
        {
            var books = await _bookRepo.SearchActiveBooksAsync(term);
            await PopulateViewDataForAdmin();
            ViewData["IsDeletedView"] = false;

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
                Category = book.Category,
                ImageUrl = book.ImageUrl
            }).ToList();

            return View("Admin", dtos);
        }

        // GET
        public async Task<IActionResult> FilterByAuthor(Guid authorId)
        {
            var books = await _bookRepo.GetBooksByAuthorAsync(authorId);

            await PopulateViewDataForAdmin();
            ViewData["IsDeletedView"] = false;

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
                Category = book.Category,
                ImageUrl = book.ImageUrl
            }).ToList();

            return View("Admin", dtos);
        }

        // GET
        public async Task<IActionResult> FilterByCategory(Guid categoryId)
        {
            var books = await _bookRepo.GetBooksByCategoryAsync(categoryId);
            await PopulateViewDataForAdmin();
            ViewData["IsDeletedView"] = false;

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
                Category = book.Category,
                ImageUrl = book.ImageUrl
            }).ToList();

            return View("Admin", dtos);
        }
        
        [HttpGet]
        public async Task<IActionResult> AddAuthor()
        {
            await PopulateViewDataForAdmin();
            await SetCartItemCount();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAuthor(AuthorDto authDto)
        {
            if (string.IsNullOrWhiteSpace(authDto.Name))
            {
                ViewData["EmptyFields"] = "Author name cannot be empty!";
                await PopulateViewDataForAdmin();
                return View(authDto);
            }

            var authors = await _authorRepo.GetAllAuthorsAsync();
            if (authors.Any(a => a.AuthorName.Equals(authDto.Name.Trim())))
            {
                ViewData["AuthorExists"] = "Author already exists!";
                await PopulateViewDataForAdmin();
                return View(authDto);
            }

            var author = new Author
            {
                Id = Guid.NewGuid(),
                AuthorName = authDto.Name.Trim()
            };

            await _authorRepo.AddAuthorAsync(author);
            TempData["Message"] = "Author added successfully!";
            return RedirectToAction("AddAuthor");
        }

        [HttpGet]
        public async Task<IActionResult> EditAuthor(Guid id)
        {
            var author = await _authorRepo.GetAuthorByIdAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            var dto = new AuthorDto
            {
                AuthorId = author.Id,
                Name = author.AuthorName
            };

            await PopulateViewDataForAdmin();
            ViewData["IsEditing"] = true;
            return View("AddAuthor",dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditAuthor(AuthorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                ViewData["EmptyFields"] = "Author name cannot be empty!";
                await PopulateViewDataForAdmin();
                ViewData["IsEditing"] = true; 
                return View("AddAuthor", dto); 
            }

            try
            {
                var author = await _authorRepo.GetAuthorByIdAsync(dto.AuthorId);
                if (author == null)
                {
                    return NotFound();
                }

                author.AuthorName = dto.Name.Trim();
                await _authorRepo.UpdateAuthorAsync(author);

                TempData["Success"] = "Author updated successfully!";
                return RedirectToAction("AddAuthor");
            }
            catch (InvalidOperationException ex)
            {
                ViewData["EmptyFields"] = ex.Message;
                await PopulateViewDataForAdmin();
                ViewData["IsEditing"] = true;
                return View("AddAuthor", dto);
            }
        }


        [HttpPost]
        public async Task<IActionResult> RemoveAuthor(Guid authorId)
        {
            try
            {
                var author = await _authorRepo.GetAuthorByIdAsync(authorId);
                if (author == null)
                {
                    TempData["Error"] = "Author not found!";
                    return RedirectToAction("AddAuthor");
                }

                var booksByAuthor = await _bookRepo.GetBooksByAuthorAsync(authorId);
                if (booksByAuthor.Any())
                {
                    TempData["Error"] = "Cannot delete author. Books are associated with this author.";
                    return RedirectToAction("AddAuthor");
                }

                await _authorRepo.DeleteAuthor(author);
                TempData["Success"] = "Author deleted successfully!"; 
                return RedirectToAction("AddAuthor");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the author.";
                return RedirectToAction("AddAuthor");
            }
        }


      //Category CRUD

        [HttpGet]
        public async Task<IActionResult> AddCategory()
        {
            ViewData["Categories"] = await _categoryRepo.GetAllCategoriesAsync();
            await SetCartItemCount();
            return View(new CategoryDto());
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryDto catDto)
        {
            if (string.IsNullOrWhiteSpace(catDto.CategoryType))
            {
                ViewData["EmptyFields"] = "Category name cannot be empty!";
                ViewData["Categories"] = await _categoryRepo.GetAllCategoriesAsync();
                return View(catDto);
            }

            var categories = await _categoryRepo.GetAllCategoriesAsync();
            if (categories.Any(c => c.CategoryType.Equals(catDto.CategoryType.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ViewData["CategoryExists"] = "A category with this name already exists!";
                ViewData["Categories"] = await _categoryRepo.GetAllCategoriesAsync();
                return View(catDto);
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                CategoryType = catDto.CategoryType.Trim(),
            };

            await _categoryRepo.AddCategoryAsync(category);
            TempData["Success"] = "Category added successfully!";
            return RedirectToAction("AddCategory");
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(Guid id)
        {
            var category = await _categoryRepo.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var dto = new CategoryDto
            {
                Id = category.Id,
                CategoryType = category.CategoryType
            };

            ViewData["Categories"] = await _categoryRepo.GetAllCategoriesAsync();
            ViewData["IsEditing"] = true;
            return View("AddCategory", dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(CategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryType))
            {
                ViewData["EmptyFields"] = "Category name cannot be empty!";
                ViewData["Categories"] = await _categoryRepo.GetAllCategoriesAsync();
                ViewData["IsEditing"] = true;
                return View("AddCategory", dto);
            }

            var category = await _categoryRepo.GetCategoryByIdAsync(dto.Id);
            if (category == null)
            {
                return NotFound();
            }

            category.CategoryType = dto.CategoryType.Trim();
            await _categoryRepo.UpdateCategoryAsync(category);

            TempData["Success"] = "Category updated successfully!";
            return RedirectToAction("AddCategory");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCategory(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
            {
                TempData["Error"] = "Invalid category ID.";
                return RedirectToAction("AddCategory");
            }

            try
            {
                var booksInCategory = await _bookRepo.GetBooksByCategoryAsync(categoryId);
                if (booksInCategory.Any())
                {
                    TempData["Error"] = $"Cannot delete this category because it is still used by {booksInCategory.Count()} book(s). Please reassign them first.";
                    return RedirectToAction("AddCategory");
                }

                var category = await _categoryRepo.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("AddCategory");
                }

                await _categoryRepo.DeleteCategoryAsync(categoryId);
                TempData["Success"] = "Category deleted successfully!";
                return RedirectToAction("AddCategory");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the category.";
                return RedirectToAction("AddCategory");
            }
        }


        //Populating data for aviwews

        private async Task PopulateViewDataForAdmin()
        {
            ViewData["Authors"] = await _authorRepo.GetAllAuthorsAsync();
            ViewData["Categories"] = await _categoryRepo.GetAllCategoriesAsync();
        }

        public async Task<IActionResult> ClearFilters()
        {
            return RedirectToAction("Admin");
        }


        private async Task SetCartItemCount()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetCurrentUserId();
                if (userId != Guid.Empty && _cartRepo != null)
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
    }
}
