using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebsite.Repository
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<Author>> GetAllAuthorsAsync();
        Task<Author?> GetAuthorByIdAsync(Guid id);
        Task AddAuthorAsync(Author author);
        Task DeleteAuthor(Author author);
        Task UpdateAuthorAsync(Author author);
        Task<Author?> GetAuthorWithBooksAsync(Guid id);
        Task<Author?> GetActiveAuthorByIdAsync(Guid id);
    }

    public class AuthorRepository : IAuthorRepository
    {
        private readonly ECommerceWebsiteDbContext _context;

        public AuthorRepository(ECommerceWebsiteDbContext context)
        {
            _context = context;
        }

        public async Task AddAuthorAsync(Author author)
        {
            if (string.IsNullOrWhiteSpace(author.AuthorName))
            {
                throw new ArgumentException("Author name cannot be null or whitespace.", nameof(author));
            }

            bool authorExists = await _context.Authors
                .AnyAsync(a => a.AuthorName.ToLower() == author.AuthorName.ToLower());

            if (authorExists)
            {
                throw new InvalidOperationException($"An author with the name '{author.AuthorName}' already exists.");
            }
   
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
        {
            return await _context.Authors.ToListAsync();
        }

        public async Task<Author?> GetAuthorByIdAsync(Guid id)
        {
            return await _context.Authors.FindAsync(id);
        }
        public async Task DeleteAuthor(Author author)
        { 
           _context.Authors.Remove(author);
           await _context.SaveChangesAsync();
        }

        public async Task UpdateAuthorAsync(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            if (string.IsNullOrWhiteSpace(author.AuthorName))
            {
                throw new ArgumentException("Author name cannot be null or whitespace.", nameof(author));
            }


            var existingAuthor = await _context.Authors.FindAsync(author.Id);
            if (existingAuthor == null)
            {
                throw new KeyNotFoundException($"Author with ID '{author.Id}' not found.");
            }

            // Check for duplicate author names
            bool duplicateExists = await _context.Authors
                .AnyAsync(a => a.Id != author.Id &&
                              a.AuthorName.ToLower() == author.AuthorName.ToLower());

            if (duplicateExists)
            {
                throw new InvalidOperationException($"An author with the name '{author.AuthorName}' already exists.");
            }

            // Update the properties
            existingAuthor.AuthorName = author.AuthorName.Trim();
            existingAuthor.UpdatedAt = DateTime.UtcNow;

            _context.Authors.Update(existingAuthor);
            await _context.SaveChangesAsync();
        }

        public async Task<Author?> GetAuthorWithBooksAsync(Guid id)
        {
            return await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Author?> GetActiveAuthorByIdAsync(Guid id)
        {
            return await _context.Authors
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

    }
}
