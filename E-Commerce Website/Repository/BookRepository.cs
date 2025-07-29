using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceWebsite.Repository
{
    public interface IBookRepository
    {
        //1. AddBook (Create)
        //2. DeleteBook (Delete)
        //3. UpdateBook (Update)
        //4. GetBookById (Read)
        //5. GetBookByTitle (List)
        //6. GetBookByAuthor (List)
        //7. GetBooksByCategory (List)
        //8. GetAllBooks (List)
        Task AddBookAsync(Book book);
        Task DeleteBookAsync(Guid id);
        Task UpdateBookAsync(Book book);
        Task<Book> GetBookByIdAsync(Guid id);
        Task<Book> GetBooksByTitle(string title);
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(string name); //IEnumerable for eager loading on home page
        Task<IEnumerable<Book>> GetBooksByCategoryAsync(string categoryName);
        Task<IEnumerable<Book>> GetAllBooksAsync(); 

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

        public async Task DeleteBookAsync(Guid id)
        {
            await GetBookByIdAsync(id);
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Book not found");
            }
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
           return await _context.Books.ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(Guid id)
        {
           return await _context.Books.FindAsync(id);
        }

        public async Task<Book> GetBooksByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title), "Title cannot be null or empty");
            }
            title = title.Trim().ToLower();
            return await _context.Books
                   .FirstOrDefaultAsync(b => b.Title.Trim().ToLower().Contains(title));
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string name)
        {
           if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "Author name cannot be null");
            }

            name = name.Trim().ToLower();
            return await _context.Books.Where(b=> b.Author != null && b.Author.AuthorName.ToLower().Contains(name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(string categoryName)
        {
            if(string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentNullException(nameof(categoryName), "Category name cannot be null or empty");
            }
            categoryName = categoryName.Trim().ToLower();
            return await _context.Books.Where(b=> b.Category.CategoryType.Trim().ToLower().Contains(categoryName))
                .ToListAsync();
        }

        public async Task UpdateBookAsync(Book book)
        {
            var existingBook = await _context.Books.FindAsync(book.Id);
            if (existingBook == null)
            {
                throw new KeyNotFoundException("Book not found");
            }
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
    }
}
