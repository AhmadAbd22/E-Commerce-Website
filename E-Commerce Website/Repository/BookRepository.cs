using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceWebsite.Repository
{
        //1. AddBook (Create)
        //2. DeleteBook (Delete)
        //3. UpdateBook (Update)
        //4. GetBookById (Read)
        //5. GetBookByTitle (List)
        //6. GetBookByAuthor (List)
        //7. GetBooksByCategory (List)
        //8. GetAllBooks (List)
        public interface IBookRepository
        {
            Task AddBookAsync(Book book);
            Task UpdateBookAsync(Book book);
            Task<Book?> GetBookByIdAsync(Guid id);
            Task<Book?> GetActiveBookByIdAsync(Guid id);
            Task<IEnumerable<Book>> GetActiveBooksAsync();
            Task<IEnumerable<Book>> GetAllBooksAsync();
            Task DeleteBookAsync(Guid id);
            Task<bool> IsBookInCartAsync(Guid bookId);
            Task<IEnumerable<Book>> SearchActiveBooksAsync(string searchTerm);
            Task<Book?> GetBooksByTitle(string title);
            Task<IEnumerable<Book>> GetBooksByAuthorAsync(string name);
            Task<IEnumerable<Book>> GetBooksByCategoryAsync(string categoryName);
            Task<int> GetTotalBooksCountAsync(); // total books
            Task<IEnumerable<Book>> GetRecentBooksAsync(int count); // latest books
            Task<IEnumerable<Book>> GetDeletedBooksAsync();
            Task<IEnumerable<Book>> GetBooksByAuthorAsync(Guid authorId);
            Task<IEnumerable<Book>> GetBooksByCategoryAsync(Guid categoryId);
            Task<IEnumerable<Book>> FilterBooksAsync(Guid? authorId, decimal? minPrice, decimal? maxPrice);
            Task RestoreBookAsync(Guid id);


    }

    public class BookRepository : IBookRepository
        {
            private readonly ECommerceWebsiteDbContext _context;

            public BookRepository(ECommerceWebsiteDbContext context)
            {
                _context = context;
            }

            public async Task AddBookAsync(Book book)
            {
                await _context.Books.AddAsync(book);
                await _context.SaveChangesAsync();
            }

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

            public async Task<Book?> GetBookByIdAsync(Guid id)
            {
                return await _context.Books.FindAsync(id);
            }

            public async Task<Book?> GetActiveBookByIdAsync(Guid id)
            {
                return await _context.Books
                    .FirstOrDefaultAsync(b => b.Id == id && b.isActive == (int)enumStatus.Active);
            }

            public async Task<IEnumerable<Book>> GetActiveBooksAsync()
            {
                return await _context.Books
                    .Where(b => b.isActive == (int)enumStatus.Active)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
            }

            public async Task<IEnumerable<Book>> GetAllBooksAsync()
            {
                return await _context.Books.ToListAsync();
            }

            public async Task DeleteBookAsync(Guid id)
            {
                var book = await _context.Books.FindAsync(id);
                if (book != null)
                {
                    book.isActive = (int)enumStatus.Inactive;
                    _context.Books.Update(book);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new KeyNotFoundException("Book not found");
                }
            }

            public async Task<bool> IsBookInCartAsync(Guid bookId)
            {
                return await _context.CartItems.AnyAsync(c => c.BookId == bookId);
            }

            public async Task<IEnumerable<Book>> SearchActiveBooksAsync(string searchTerm)
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetActiveBooksAsync();

                searchTerm = searchTerm.Trim().ToLower();

                return await _context.Books
                    .Where(b => b.isActive == (int)enumStatus.Active &&
                           (b.Title.ToLower().Contains(searchTerm) ||
                            b.Description.ToLower().Contains(searchTerm)))
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
            }

            public async Task<Book?> GetBooksByTitle(string title)
            {
                if (string.IsNullOrWhiteSpace(title))
                    throw new ArgumentNullException(nameof(title));

                title = title.Trim().ToLower();

                return await _context.Books
                    .FirstOrDefaultAsync(b => b.Title.ToLower().Contains(title));
            }

            public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name));

                name = name.Trim().ToLower();

                return await _context.Books
                    .Where(b => b.Author != null &&
                                b.Author.AuthorName.ToLower().Contains(name))
                    .ToListAsync();
            }

            public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(string categoryName)
            {
                if (string.IsNullOrWhiteSpace(categoryName))
                    throw new ArgumentNullException(nameof(categoryName));

                categoryName = categoryName.Trim().ToLower();

                return await _context.Books
                    .Where(b => b.Category != null &&
                                b.Category.CategoryType.ToLower().Contains(categoryName))
                    .ToListAsync();
            }

            public async Task<int> GetTotalBooksCountAsync()
            {
                return await _context.Books.CountAsync();
            }

            public async Task<IEnumerable<Book>> GetRecentBooksAsync(int count)
            {
                return await _context.Books
                    .Where(b => b.isActive == (int)enumStatus.Active)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }

        public async Task<IEnumerable<Book>> GetDeletedBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Inactive )
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

        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(Guid categoryId)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.isActive == (int)enumStatus.Active && b.CategoryId == categoryId)
                .ToListAsync();
        }

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

            return await query.ToListAsync();
        }


        public async Task RestoreBookAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.isActive = (int)enumStatus.Active;
                book.DeletedAt = null; //clear deleteion date 
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Book to restore not found");
            }
        }

    }
}

