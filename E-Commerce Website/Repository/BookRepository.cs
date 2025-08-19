using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Enums;
using ECommerceWebsite.Models.Helping_Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceWebsite.Repository
{
    public interface IBookRepository
    {
        #region CRUD Operations
        // CREATE
        Task AddBookAsync(Book book);

        // READ
        Task<Book?> GetBookByIdAsync(Guid id);
        Task<Book?> GetActiveBookByIdAsync(Guid id);
        Task<IEnumerable<Book>> GetActiveBooksAsync();
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetBooksByTitle(string title);
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(string name);
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(Guid authorId);
        Task<IEnumerable<Book>> GetBooksByCategoryAsync(string categoryName);
        Task<IEnumerable<Book>> GetBooksByCategoryAsync(Guid categoryId);
        Task<IEnumerable<Book>> GetDeletedBooksAsync();
        Task<IEnumerable<Book>> GetRecentBooksAsync(int count);
        Task<int> GetTotalBooksCountAsync();
        Task<bool> IsBookInCartAsync(Guid bookId);

        // UPDATE
        Task UpdateBookAsync(Book book);
        Task RestoreBookAsync(Guid id);

        // DELETE
        Task DeleteBookAsync(Guid id);
        #endregion

        #region Search, Filter, Pagination & Sorting
        // Search Operations
        Task<IEnumerable<Book>> SearchActiveBooksAsync(string searchTerm);
        Task<PagedResult<Book>> SearchActiveBooksPagedAsync(string searchTerm, int pageNumber, int pageSize);

        // Filter Operations
        Task<IEnumerable<Book>> FilterBooksAsync(Guid? authorId, decimal? minPrice, decimal? maxPrice);
        Task<PagedResult<Book>> FilterBooksPagedAsync(Guid? authorId, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize);

        // Pagination Operations
        Task<PagedResult<Book>> GetActiveBooksPagedAsync(int pageNumber, int pageSize);

        // Sorting Operations
        Task<PagedResult<Book>> SortBooksPagedAsync(string sortBy, string sortOrder, int pageNumber, int pageSize);

        // Combined Filter and Sort Operations
        Task<PagedResult<Book>> FilterAndSortBooksPagedAsync(
                                    Guid? authorId, Guid? categoryId, decimal? minPrice, decimal? maxPrice,
                                    string sortBy, string sortOrder, int pageNumber, int pageSize);
        #endregion
    }

    public class BookRepository : IBookRepository
    {
        private readonly ECommerceWebsiteDbContext _context;

        public BookRepository(ECommerceWebsiteDbContext context)
        {
            _context = context;
        }

        #region CRUD Operations

        #region CREATE
        public async Task AddBookAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }
        #endregion

        #region READ
        public async Task<Book?> GetBookByIdAsync(Guid id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<Book?> GetActiveBookByIdAsync(Guid id)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.isActive == (int)enumStatus.Active);
        }

        public async Task<IEnumerable<Book>> GetActiveBooksAsync()
        {
            return await _context.Books
                .Where(b => b.isActive == (int)enumStatus.Active)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToListAsync();
        }

        public async Task<Book?> GetBooksByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));

            title = title.Trim().ToLower();

            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Title.ToLower().Contains(title));
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            name = name.Trim().ToLower();

            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Author != null &&
                            b.Author.AuthorName.ToLower().Contains(name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(Guid authorId)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Active && b.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ArgumentNullException(nameof(categoryName));

            categoryName = categoryName.Trim().ToLower();

            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Category != null &&
                            b.Category.CategoryType.ToLower().Contains(categoryName))
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(Guid categoryId)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Active && b.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetDeletedBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Inactive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetRecentBooksAsync(int count)
        {
            return await _context.Books
                .Where(b => b.isActive == (int)enumStatus.Active)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetTotalBooksCountAsync()
        {
            return await _context.Books
                .Where(b => b.isActive == (int)enumStatus.Active)
                .CountAsync();
        }

        public async Task<bool> IsBookInCartAsync(Guid bookId)
        {
            return await _context.CartItems.AnyAsync(c => c.BookId == bookId);
        }
        #endregion

        #region UPDATE
        public async Task UpdateBookAsync(Book book)
        {
            var existingBook = await _context.Books.FindAsync(book.Id);
            if (existingBook == null)
                throw new KeyNotFoundException("Book not found");

            existingBook.Title = book.Title;
            existingBook.Description = book.Description;
            existingBook.Price = book.Price;
            existingBook.StockQuantity = book.StockQuantity;
            existingBook.CategoryId = book.CategoryId;
            existingBook.AuthorId = book.AuthorId;
            existingBook.ImageUrl = book.ImageUrl;
            existingBook.PublicationDate = book.PublicationDate;

            await _context.SaveChangesAsync();
        }

        public async Task RestoreBookAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.isActive = (int)enumStatus.Active;
                book.DeletedAt = null;
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Book to restore not found");
            }
        }
        #endregion

        #region DELETE
        public async Task DeleteBookAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.isActive = (int)enumStatus.Inactive;
                book.DeletedAt = DateTime.UtcNow;
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Book not found");
            }
        }
        #endregion

        #endregion

        #region Search, Filter, Pagination & Sorting

        #region Search Operations
        public async Task<IEnumerable<Book>> SearchActiveBooksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveBooksAsync();

            searchTerm = searchTerm.Trim().ToLower();

            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Active &&
                       (b.Title.ToLower().Contains(searchTerm) ||
                       (b.Author != null && b.Author.AuthorName.ToLower().Contains(searchTerm))))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<Book>> SearchActiveBooksPagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveBooksPagedAsync(pageNumber, pageSize);

            searchTerm = searchTerm.Trim().ToLower();

            var query = _context.Books
                .Where(b => b.isActive == (int)enumStatus.Active &&
                       (b.Title.ToLower().Contains(searchTerm) ||
                       (b.Author != null && b.Author.AuthorName.ToLower().Contains(searchTerm))))
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Book>
            {
                Items = items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        #endregion

        #region Filter Operations
        public async Task<IEnumerable<Book>> FilterBooksAsync(Guid? authorId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Active)
                .AsQueryable();

            if (authorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == authorId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(b => b.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= maxPrice.Value);
            }

            return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        public async Task<PagedResult<Book>> FilterBooksPagedAsync(Guid? authorId, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Active)
                .AsQueryable();

            if (authorId.HasValue && authorId != Guid.Empty)
            {
                query = query.Where(b => b.AuthorId == authorId.Value);
            }
            if (categoryId.HasValue && categoryId != Guid.Empty)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }
            if (minPrice.HasValue)
            {
                query = query.Where(b => b.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= maxPrice.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(b => b.CreatedAt).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Book>
            {
                Items = items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        #endregion

        #region Pagination Operations
        public async Task<PagedResult<Book>> GetActiveBooksPagedAsync(int pageNumber = 1, int pageSize = 9)
        {
            var query = _context.Books
                .Where(b => b.isActive == (int)enumStatus.Active)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            return new PagedResult<Book>
            {
                Items = items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        #endregion

        #region Sorting Operations
        public async Task<PagedResult<Book>> SortBooksPagedAsync(string sortBy, string sortOrder, int pageNumber, int pageSize)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Active)
                .AsQueryable();

            // Apply sorting based on sortBy parameter
            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("title", "desc") => query.OrderByDescending(b => b.Title).ThenByDescending(b => b.CreatedAt),
                ("title", "asc") => query.OrderBy(b => b.Title).ThenByDescending(b => b.CreatedAt),

                ("author", "desc") => query.OrderByDescending(b => b.Author != null ? b.Author.AuthorName : "").ThenByDescending(b => b.CreatedAt),
                ("author", "asc") => query.OrderBy(b => b.Author != null ? b.Author.AuthorName : "").ThenByDescending(b => b.CreatedAt),

                ("category", "desc") => query.OrderByDescending(b => b.Category != null ? b.Category.CategoryType : "").ThenByDescending(b => b.CreatedAt),
                ("category", "asc") => query.OrderBy(b => b.Category != null ? b.Category.CategoryType : "").ThenByDescending(b => b.CreatedAt),

                ("price", "desc") => query.OrderByDescending(b => b.Price).ThenByDescending(b => b.CreatedAt),
                ("price", "asc") => query.OrderBy(b => b.Price).ThenByDescending(b => b.CreatedAt),

                ("date", "desc") => query.OrderByDescending(b => b.CreatedAt),
                ("date", "asc") => query.OrderBy(b => b.CreatedAt),

                _ => query.OrderByDescending(b => b.CreatedAt) // Default sorting (Latest first)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Book>
            {
                Items = items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        #endregion

        #region Combined Filter and Sort Operations
        public async Task<PagedResult<Book>> FilterAndSortBooksPagedAsync(
            Guid? authorId, Guid? categoryId, decimal? minPrice, decimal? maxPrice,
            string sortBy, string sortOrder, int pageNumber, int pageSize)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Active)
                .AsQueryable();

            // Apply Filters (if any)
            if (authorId.HasValue && authorId != Guid.Empty)
            {
                query = query.Where(b => b.AuthorId == authorId.Value);
            }

            if (categoryId.HasValue && categoryId != Guid.Empty)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(b => b.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= maxPrice.Value);
            }

            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("title", "desc") => query.OrderByDescending(b => b.Title).ThenByDescending(b => b.CreatedAt),
                ("title", "asc") => query.OrderBy(b => b.Title).ThenByDescending(b => b.CreatedAt),

                ("author", "desc") => query.OrderByDescending(b => b.Author != null ? b.Author.AuthorName : "").ThenByDescending(b => b.CreatedAt),
                ("author", "asc") => query.OrderBy(b => b.Author != null ? b.Author.AuthorName : "").ThenByDescending(b => b.CreatedAt),

                ("category", "desc") => query.OrderByDescending(b => b.Category != null ? b.Category.CategoryType : "").ThenByDescending(b => b.CreatedAt),
                ("category", "asc") => query.OrderBy(b => b.Category != null ? b.Category.CategoryType : "").ThenByDescending(b => b.CreatedAt),

                ("price", "desc") => query.OrderByDescending(b => b.Price).ThenByDescending(b => b.CreatedAt),
                ("price", "asc") => query.OrderBy(b => b.Price).ThenByDescending(b => b.CreatedAt),

                ("date", "desc") => query.OrderByDescending(b => b.CreatedAt),
                ("date", "asc") => query.OrderBy(b => b.CreatedAt),

                _ => query.OrderByDescending(b => b.CreatedAt) // Default sorting
            };

            // Execute with Pagination
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Book>
            {
                Items = items,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        #endregion

        #endregion
    }
}
