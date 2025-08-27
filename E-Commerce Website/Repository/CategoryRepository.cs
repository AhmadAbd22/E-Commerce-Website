using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Helping_Classes;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebsite.Models.Repository
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync(); //READ
        Task<Category?> GetCategoryByIdAsync(Guid id); 
        Task AddCategoryAsync(Category category);           //CREATE
        Task UpdateCategoryAsync(Category category);        //UPDATE
        Task DeleteCategoryAsync(Guid id);                  //DELETE

        Task<PagedResult<Category>> GetAllCategoriesPagedAsync(string search, string sortBy, int pageNumber, int pageSize);  // Pagination 

    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly ECommerceWebsiteDbContext _context;

        public CategoryRepository(ECommerceWebsiteDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PagedResult<Category>> GetAllCategoriesPagedAsync(string search, string sortBy, int pageNumber, int pageSize)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.CategoryType.ToLower().Contains(search.ToLower()));
            }

            switch (sortBy)
            {
                case "name_desc":
                    query = query.OrderByDescending(c => c.CategoryType);
                    break;
                default: 
                    query = query.OrderBy(c => c.CategoryType);
                    break;
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Category>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };

        }
    }
}
