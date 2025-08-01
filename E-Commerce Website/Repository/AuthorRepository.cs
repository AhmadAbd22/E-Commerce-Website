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
    }
}
