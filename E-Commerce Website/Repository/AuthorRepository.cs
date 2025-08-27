using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Helping_Classes;
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
        Task<PagedResult<Author>> GetAllAuthorsPagedAsync(string search, string sortBy, int pageNumber, int pageSize);
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

        public async Task<PagedResult<Author>> GetAllAuthorsPagedAsync(string search, string sortBy, int pageNumber, int pageSize)
        {
            var query = _context.Authors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.AuthorName.ToLower().Contains(search.ToLower()));
            }

            switch (sortBy)
            {
                case "name_desc":
                    query = query.OrderByDescending(a => a.AuthorName);
                    break;
                default: // "name_asc" or any other value
                    query = query.OrderBy(a => a.AuthorName);
                    break;
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Author>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }
    }
}