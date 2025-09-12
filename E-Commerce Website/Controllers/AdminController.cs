using ECommerceWebsite.Hubs;
using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Dtos;
using ECommerceWebsite.Models.Enums;
using ECommerceWebsite.Models.Helping_Classes;
using ECommerceWebsite.Models.Repository;
using ECommerceWebsite.Repository;
using ECommerceWebsite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;


namespace ECommerceWebsite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        #region dependencies and constructors
        /// Dependance Injection
        private readonly IBookRepository _bookRepo;
        private readonly IAuthorRepository _authorRepo;
        private readonly ICategoryRepository _categoryRepo;

        private readonly IOrderRepository _orderRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICartRepository _cartRepo;

        private readonly Authorization _authorization;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminController(IBookRepository bookRepo, IAuthorRepository authorRepo,
                                ICategoryRepository categoryRepo, ICartRepository cartRepo,
                                IOrderRepository orderRepo,
                                IUserRepository userRepo,
                                Authorization authorization, IHubContext<NotificationHub> hubContext)
        {
            _bookRepo = bookRepo;
            _authorRepo = authorRepo;
            _categoryRepo = categoryRepo;
            _cartRepo = cartRepo;
            _orderRepo = orderRepo;
            _userRepo = userRepo;
            _authorization = authorization;
            _hubContext = hubContext;
        }

        #endregion

        #region Book Management
        // GET: /Admin
        public async Task<IActionResult> Admin(string search, Guid? authorId, Guid? categoryId,
                                                 decimal? minPrice, decimal? maxPrice,
                                                 string sortBy, string sortOrder,
                                                 int pageNumber = 1, int pageSize = 10)
        {
            PagedResult<Book> pagedBooks;

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Search only (no filters or sorting applied to search results)
                pagedBooks = await _bookRepo.SearchActiveBooksPagedAsync(search.Trim(), pageNumber, pageSize);
            }
            else
            {
                // Combined Filter + Sort (or just filter, or just sort, or default)
                pagedBooks = await _bookRepo.FilterAndSortBooksPagedAsync(
                    authorId, categoryId, minPrice, maxPrice,
                    sortBy, sortOrder, pageNumber, pageSize);
            }

            var pagedDto = new PagedResult<BookDto>
            {
                CurrentPage = pagedBooks.CurrentPage,
                PageSize = pagedBooks.PageSize,
                TotalCount = pagedBooks.TotalCount,
                Items = [.. pagedBooks.Items.Select(book => new BookDto
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
                })] //.ToList() using ' [.. ' for simplicity
            };

            await PopulateViewDataForAdmin();

            // Preserve all filter/sort state
            ViewData["Search"] = search;
            ViewData["AuthorId"] = authorId;
            ViewData["CategoryId"] = categoryId;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_AdminBookGridPartial", pagedDto);
            }

            return View(pagedDto);
        }


        [HttpGet]
        public async Task<IActionResult> BookDetailsPartial(Guid id)
        {
            var book = await _bookRepo.GetBookByIdAsync(id);
            if (book == null)
            {
                return Content("<div class='alert alert-danger'>Book not found.</div>");
            }

            await PopulateViewDataForAdmin();

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

            return PartialView("_BookDetailsModalPartial", bookDto);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            var authors = await _authorRepo.GetAllAuthorsAsync();

            await SetCartItemCount();

            var dto = new BookDto
            {
                CategoriesList = [.. categories],
                AuthorsList = [.. authors]
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

                await PopulateViewDataForAdmin();
                return View(dto);
            }

            //Check if the title exists already
            var existingBook = await _bookRepo.GetBookByTitle(dto.Title, dto.Author?.AuthorName ?? "");
            if (existingBook != null)
            {
                TempData.SetInfo("A book with this title already exists.");
                await PopulateViewDataForAdmin();
                return View(dto);
            }

            var bookId = Guid.NewGuid();

            //Create directory 
            GeneralPurpose.CreateBookDirectory(bookId);

            var fileName = string.IsNullOrWhiteSpace(dto.FileName)
                ? Path.GetFileName(dto.ImageFile.FileName)
                : dto.FileName + Path.GetExtension(dto.ImageFile.FileName);

            //Get file path and save
            var filePath = GeneralPurpose.GetBookImagePathForSave(bookId, fileName);
            var imageSaved = await GeneralPurpose.SaveFile(dto.ImageFile, filePath);

            if (!imageSaved)
            {
                ModelState.AddModelError("ImageFile", "Failed to save image.");
                dto.CategoriesList = [.. (await _categoryRepo.GetAllCategoriesAsync())];
                dto.AuthorsList = [.. (await _authorRepo.GetAllAuthorsAsync())];
                return View(dto);
            }

            //Get URL for DB
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
            TempData.SetSuccess($"<i class='{NotificationIcons.Success}'></i> '{book.Title}' has been added to the Catalog");
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"New book '{book.Title} by {book.Author}' added!");
            return RedirectToAction("Admin");
        }

        // GEt
        public async Task<IActionResult> Edit(Guid id)
        {
            var book = await _bookRepo.GetBookByIdAsync(id);
            if (book == null) 
            {
                TempData.SetInfo("Error editing Book." );
                return RedirectToAction("Admin");
            }

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
                ISBN = book.ISBN,
                PublicationDate = book.PublicationDate,
                FileName = book.FileName,
            };

            await _bookRepo.UpdateBookAsync(book);

            return View(dto);
        }



        // POST: 
        [HttpPost]
        public async Task<IActionResult> Edit(BookDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewDataForAdmin();
                return View(dto);
            }

            var book = await _bookRepo.GetBookByIdAsync(dto.Id);
            if (book == null)
                return NotFound();

            // Handle image update if new file is provided
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                // Validate new image
                if (!GeneralPurpose.IsValidImageExtension(dto.ImageFile.FileName))
                {
                    ModelState.AddModelError("ImageFile", "Only PNG, JPG and JPEG images are allowed.");
                    await PopulateViewDataForAdmin();
                    return View(dto);
                }

                if (!GeneralPurpose.IsValidImageSize(dto.ImageFile))
                {
                    ModelState.AddModelError("ImageFile", "File size must be less than 2MB.");
                    await PopulateViewDataForAdmin();
                    return View(dto);
                }

                // Delete old image if it exists
                if (!string.IsNullOrEmpty(book.FileName))
                {
                    await GeneralPurpose.DeleteBookImage(book.Id, book.FileName);
                }

                // Save new image
                var fileName = string.IsNullOrWhiteSpace(dto.FileName)
                    ? Path.GetFileName(dto.ImageFile.FileName)
                    : dto.FileName + Path.GetExtension(dto.ImageFile.FileName);

                var filePath = GeneralPurpose.GetBookImagePathForSave(book.Id, fileName);
                var imageSaved = await GeneralPurpose.SaveFile(dto.ImageFile, filePath);

                if (!imageSaved)
                {
                    ModelState.AddModelError("ImageFile", "Failed to save image.");
                    await PopulateViewDataForAdmin();
                    return View(dto);
                }

                // Update image properties
                book.ImageUrl = GeneralPurpose.GetBookImageUrl(book.Id, fileName);
                book.Path = filePath;
                book.FileName = fileName;
            }

            // Update other properties
            book.Title = dto.Title;
            book.Description = dto.Description;
            book.Price = dto.Price;
            book.StockQuantity = dto.Stock;
            book.AuthorId = dto.AuthorId;
            book.CategoryId = dto.CategoryId;
            book.PublicationDate = dto.PublicationDate;
            book.UpdatedAt = DateTime.UtcNow;

            await _bookRepo.UpdateBookAsync(book);
            TempData.SetSuccess("Book updated successfully!");
            return RedirectToAction("Admin");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var book = await _bookRepo.GetBookByIdAsync(id);
            if (book == null)
            {
                TempData.SetError("Book not found.");
                return RedirectToAction("Admin");
            }
            await _bookRepo.DeleteBookAsync(id);
            TempData.SetSuccess($"Book {book.Title} deleted successfully!");
            return RedirectToAction("Admin");
        }


        public async Task<IActionResult> DeletedBooks(string search, Guid? authorId, Guid? categoryId,
                                                 decimal? minPrice, decimal? maxPrice,
                                                 string sortBy, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            PagedResult<Book> pagedBooks;

            if (!string.IsNullOrWhiteSpace(search))
            {
                pagedBooks = await _bookRepo.SearchDeletedBooksPagedAsync(search.Trim(), pageNumber, pageSize);
            }
            else
            {
                pagedBooks = await _bookRepo.FilterAndSortDeletedBooksPagedAsync(
                    authorId, categoryId, minPrice, maxPrice,
                    sortBy, sortOrder, pageNumber, pageSize);
            }

            await PopulateViewDataForAdmin();

            ViewData["Search"] = search;
            ViewData["AuthorId"] = authorId;
            ViewData["CategoryId"] = categoryId;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;
            ViewData["IsDeletedView"] = true;

            var pagedDto = new PagedResult<BookDto>
            {
                CurrentPage = pagedBooks.CurrentPage,
                PageSize = pagedBooks.PageSize,
                TotalCount = pagedBooks.TotalCount,
                Items = [.. pagedBooks.Items.Select(book => new BookDto
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
                })]
            };

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_AdminBookGridPartial", pagedDto);
            }

            return View(pagedDto);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreBook(Guid id)
        {
            var book = await _bookRepo.GetBookByIdAsync(id);
            if(book == null)
            {
                TempData.SetError("Error restoring book.");
                return RedirectToAction("DeletedBooks");
            }
            
            await _bookRepo.RestoreBookAsync(id);
            TempData.SetSuccess("Book restored successfully!");
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Book '{book.Title} by {book.Author}' back in Stock");
            return RedirectToAction("DeletedBooks");
        }
        #endregion


        #region Dashboard and Analytics
        public async Task<IActionResult> Dashboard()
        {
            var totalSales = await _orderRepo.GetTotalSales();
            var pendingOrders = await _orderRepo.GetTotalPendingOrders();
            var completedOrders = await _orderRepo.GetTotalCompletedOrders();
            var cancelledOrders = await _orderRepo.GetTotalCancelledOrders();
            var activeUsers = await _userRepo.GetTotalUsers();

            // Populate the strongly-typed DTO directly
            var dashboardDto = new DashboardDto
            {
                TotalSales = totalSales,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                ActiveCustomers = activeUsers
            };

            return View(dashboardDto);
        }


        [HttpGet]
        public async Task<IActionResult> GetWeeklySalesByWeekData()
        {
            var weeklySales = await _orderRepo.GetWeeklySalesByWeek();

            var chartData = new ChartDataDto
            {
                Labels = [.. weeklySales.Select(w => $"Week {w.WeekNumber} ({w.StartDate:yyyy-MMM-dd})")],
                Values = [.. weeklySales.Select(w => w.TotalSales)]
            };

            return Json(chartData);
        }

        [HttpGet]
        public async Task<IActionResult> GetWeeklySalesByDayData(int isoYear, int isoWeek)
        {
            var dailySales = await _orderRepo.GetWeeklySalesByDay(isoYear, isoWeek);

            var chartData = new ChartDataDto
            {
                Labels = [.. dailySales.Select(d => d.Date.ToString("yyyy-MMM-dd"))],
                Values = [.. dailySales.Select(d => d.TotalSales)]
            };

            return Json(chartData);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableWeeks()
        {
            var weeks = await _orderRepo.GetWeeklySalesByWeek();

            var weekList = weeks.Select(w => new
            {
                Year = w.Year,
                Week = w.WeekNumber,
                Label = $"Week {w.WeekNumber} ({w.StartDate:MMM dd} - {w.EndDate:MMM dd})"
            }).ToList();

            return Json(weekList);
        }

        #endregion


        #region Author and Category Management
        [HttpGet]
        public async Task<IActionResult> AddAuthor(string search, string sortBy = "name_asc", int pageNumber = 1, int pageSize = 10)
        {
            var pagedAuthors = await _authorRepo.GetAllAuthorsPagedAsync(search, sortBy, pageNumber, pageSize);

            ViewData["CurrentSearch"] = search;
            ViewData["CurrentSortBy"] = sortBy;

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_AuthorListPartial", pagedAuthors);
            }

            ViewData["ExistingAuthors"] = pagedAuthors;

            return View(new AuthorDto());
        }


        [HttpPost]
        public async Task<IActionResult> AddAuthor(AuthorDto authDto)
        {
            if (string.IsNullOrWhiteSpace(authDto.Name) || (await _authorRepo.GetAllAuthorsAsync()).Any(a => a.AuthorName.Equals(authDto.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                TempData.SetError(string.IsNullOrWhiteSpace(authDto.Name)
                    ? "Author name cannot be empty!"
                    : "Author already exists!");

                return RedirectToAction("AddAuthor");
            }

            var newAuthor = new Author
            {
                Id = Guid.NewGuid(),
                AuthorName = authDto.Name.Trim(),
                isActive = (int)enumStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _authorRepo.AddAuthorAsync(newAuthor);
            TempData.SetSuccess("Author added successfully!");
            return RedirectToAction("AddAuthor");
        }
        [HttpGet]
        public async Task<IActionResult> EditAuthor(Guid id)
        {
            var author = await _authorRepo.GetAuthorByIdAsync(id);
            if (author == null) return NotFound();

            ViewData["ExistingAuthors"] = await _authorRepo.GetAllAuthorsPagedAsync("", "name_asc", 1, 10);
            ViewData["IsEditing"] = true;

            await _authorRepo.UpdateAuthorAsync(author);

            return View("AddAuthor", author);
        }

        [HttpPost]
        public async Task<IActionResult> EditAuthor(AuthorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                ViewData["EmptyFields"] = "Author name cannot be empty!";
                ViewData["ExistingAuthors"] = await _authorRepo.GetAllAuthorsPagedAsync("", "name_asc", 1, 10);
                ViewData["IsEditing"] = true;
                return View("AddAuthor", dto);
            }

            try
            {
                var author = await _authorRepo.GetAuthorByIdAsync(dto.AuthorId);
                if (author == null) return NotFound();
                author.AuthorName = dto.Name.Trim();
                author.UpdatedAt = DateTime.UtcNow;

                await _authorRepo.UpdateAuthorAsync(author);
            }
            catch (InvalidOperationException ex)
            {
                ViewData["EmptyFields"] = ex.Message;
                ViewData["ExistingAuthors"] = await _authorRepo.GetAllAuthorsPagedAsync("", "name_asc", 1, 10);
                ViewData["IsEditing"] = true;
                return View("AddAuthor", dto);
            }

            TempData.SetSuccess("Author updated successfully!");
            return RedirectToAction("AddAuthor");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAuthor(Guid authorId)
        {
            try
            {
                var author = await _authorRepo.GetAuthorByIdAsync(authorId);
                if (author == null)
                {
                    TempData.SetError("Author not found!");
                    return RedirectToAction("AddAuthor");
                }

                var booksByAuthor = await _bookRepo.GetBooksByAuthorAsync(authorId);
                if (booksByAuthor.Any())
                {
                    TempData.SetError("Cannot delete author. Books are associated with this author.");
                    return RedirectToAction("AddAuthor");
                }

                await _authorRepo.DeleteAuthor(author);
                TempData.SetSuccess("Author deleted successfully!");
                return RedirectToAction("AddAuthor");
            }
            catch (Exception ex)
            {
                TempData.SetError("An error occurred while deleting the author.");
                return RedirectToAction("AddAuthor");
            }
        }


        //Category CRUD

        [HttpGet]
        public async Task<IActionResult> AddCategory(string search, string sortBy = "name_asc", int pageNumber = 1, int pageSize = 10)
        {
            var pagedCategories = await _categoryRepo.GetAllCategoriesPagedAsync(search, sortBy, pageNumber, pageSize);

            // Preserve state for the view's controls
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentSortBy"] = sortBy;

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_CategoryListPartial", pagedCategories);
            }

            ViewData["ExistingCategories"] = pagedCategories;

            return View(new CategoryDto());
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryDto catDto)
        {
            if (string.IsNullOrWhiteSpace(catDto.CategoryType) || (await _categoryRepo.GetAllCategoriesAsync()).Any(c => c.CategoryType.Equals(catDto.CategoryType.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                if (string.IsNullOrWhiteSpace(catDto.CategoryType))
                    ViewData["EmptyFields"] = "Category name cannot be empty!";
                else
                    ViewData["CategoryExists"] = "A category with this name already exists!";

                // IMPORTANT: Reload the list before returning the view on failure
                ViewData["ExistingCategories"] = await _categoryRepo.GetAllCategoriesPagedAsync("", "name_asc", 1, 10);
                return View(catDto);
            }

            var newCategory = new Category
            {
                CategoryType = catDto.CategoryType.Trim(),
                isActive = (int)enumStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,

            };

            await _categoryRepo.AddCategoryAsync(newCategory);
            TempData.SetSuccess("Category added successfully!");
            return RedirectToAction("AddCategory");
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(Guid id)
        {
            var category = await _categoryRepo.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            ViewData["ExistingCategories"] = await _categoryRepo.GetAllCategoriesPagedAsync("", "name_asc", 1, 10);
            ViewData["IsEditing"] = true;

            var dto = new CategoryDto { Id = category.Id, CategoryType = category.CategoryType, };

            return View("AddCategory", dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(CategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryType))
            {
                ViewData["EmptyFields"] = "Category name cannot be empty!";
                ViewData["ExistingCategories"] = await _categoryRepo.GetAllCategoriesPagedAsync("", "name_asc", 1, 10);
                ViewData["IsEditing"] = true;
                return View("AddCategory", dto);
            }
            var existingCategory = await _categoryRepo.GetCategoryByIdAsync(dto.Id);
            if (existingCategory != null)
            {
                existingCategory.CategoryType = dto.CategoryType.Trim();
                existingCategory.UpdatedAt = DateTime.UtcNow;
                await _categoryRepo.UpdateCategoryAsync(existingCategory);
                TempData.SetSuccess("Category updated successfully!");
            }
            else
            {
                TempData.SetError("Category not found!");
            }
            return RedirectToAction("AddCategory");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCategory(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
            {
                TempData.SetError("Invalid category ID.");
                return RedirectToAction("AddCategory");
            }

            try
            {
                var booksInCategory = await _bookRepo.GetBooksByCategoryAsync(categoryId);
                if (booksInCategory.Any())
                {
                    TempData.SetError($"Cannot delete this category because it is still used by {booksInCategory.Count()} book(s). Please reassign them first.");
                    return RedirectToAction("AddCategory");
                }

                var category = await _categoryRepo.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    TempData.SetError("Category not found.");
                    return RedirectToAction("AddCategory");
                }

                await _categoryRepo.DeleteCategoryAsync(categoryId);
                TempData.SetSuccess("Category deleted successfully!");
                return RedirectToAction("AddCategory");
            }
            catch (Exception ex)
            {
                TempData.SetError("An error occurred while deleting the category.");
                return RedirectToAction("AddCategory");
            }
        }

        #endregion


        #region Order Management
        [HttpGet]
        public async Task<IActionResult> Orders(int pageNumber = 1, int pageSize = 10, string? status = null, string? sortBy = null)
        {
            var pagedOrders = await _orderRepo.GetAllOrdersPaged(pageNumber, pageSize, status, sortBy);

            ViewData["CurrentStatus"] = status;
            ViewData["CurrentSortBy"] = sortBy;
            ViewData["CurrentPageSize"] = pageSize;

            var orderDtos = pagedOrders.Items.Select(o => new AdminOrderViewDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                UserFullName = (o.User != null) ? $"{o.User.FirstName} {o.User.LastName}" : "N/A",
                ShippingAddress = o.ShippingAddress,
                City = o.City,
                PhoneNumber = o.PhoneNumber,
                TotalAmount = o.TotalAmount,
                TotalQuantity = o.OrderItems?.Sum(i => i.Quantity) ?? 0,
                OrderItems = o.OrderItems?.Select(oi => new AdminOrderItemDto
                {
                    BookTitle = oi.Book?.Title ?? "Book Deleted",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList() ?? new List<AdminOrderItemDto>()
            }).ToList();

            var pagedResultDto = new PagedResult<AdminOrderViewDto>
            {
                Items = orderDtos,
                CurrentPage = pagedOrders.CurrentPage,
                PageSize = pagedOrders.PageSize,
                TotalCount = pagedOrders.TotalCount
            };

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView("_OrdersListPartial", pagedResultDto);
            }

            return View(pagedResultDto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, string status, string sortBy)
        {
            if (orderId == Guid.Empty || string.IsNullOrEmpty(status))
            {
                TempData.SetError("Invalid order data.");
                return RedirectToAction("Orders");
            }

            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                TempData.SetError("Order not found.");
                return RedirectToAction("Orders");
            }

            var previousStatus = order.OrderStatus;
            order.OrderStatus = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepo.UpdateOrderAsync(order);

            ViewData["CurrentStatus"] = status;
            ViewData["SortBy"] = sortBy;
            TempData.SetSuccess($"Order status has been updated to '{status}'.");

            //Notify user and admins about the status update
            await _hubContext.Clients.User(order.UserId.ToString()).SendAsync("ReceiveOrderStatusUpdate", $"Order {orderId.ToString("N")[..8].ToUpper()} updated.");   //Replaced Substring(0,8) to Range Operator for simplicity
            await _hubContext.Clients.Group("Admins").SendAsync("AdminNotification", $"Order {orderId} updated.");

            return RedirectToAction("Orders");
        }

        #endregion



        #region Helper Methods

        //Populating data for views
        private async Task PopulateViewDataForAdmin()
        {
            ViewData["Authors"] = await _authorRepo.GetAllAuthorsAsync();
            ViewData["Categories"] = await _categoryRepo.GetAllCategoriesAsync();

            await SetCartItemCount();
        }
        


        private async Task SetCartItemCount()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _authorization.GetCurrentUserId();
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

        #endregion
    }
}
