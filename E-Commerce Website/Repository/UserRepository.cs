using ECommerceWebsite.Models;
using ECommerceWebsite.Models.Context;
using ECommerceWebsite.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebsite.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();                 //READ
        Task<User?> GetUserByIdAsync(Guid id);
        Task AddUserAsync(User user);                               //CREATE
        Task UpdateUserAsync(User user);                            //UPDATE        
        Task DeleteUserAsync(Guid id);                              //DELETE
        Task<User?> GetUserByEmailAsync(string email);              //READ
        Task<User?> GetUserByUsernameAsync(string username);        //READ by username
        Task<int> GetTotalUsers();
    }

    public class UserRepository : IUserRepository
    {
        private readonly ECommerceWebsiteDbContext _context;

        public UserRepository(ECommerceWebsiteDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await GetUserByIdAsync(id);
            if (user != null)
            {
                user.isActive = (int)enumStatus.Inactive; // Soft delete by setting isActive to Inactive
                user.IsDeleted = true;                    // Mark as deleted
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email 
                                    && u.isActive == (int)enumStatus.Active
                                    && !u.IsDeleted);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await GetUserByIdAsync(user.Id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Username = user.Username;
            existingUser.Password = user.Password;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Address = user.Address;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.City = user.City;
            existingUser.Province = user.Province;
            existingUser.PostalCode = user.PostalCode;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        //method for dashboard charts
        public async Task<int> GetTotalUsers()
        {
            return await _context.Users
                            .CountAsync(u => u.Role != (int)enumRole.Admin && u.isActive == (int)enumStatus.Active);
        }
    }
}
