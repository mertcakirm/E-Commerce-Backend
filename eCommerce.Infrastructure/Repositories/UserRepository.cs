using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
        private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllUsers(string? searchTerm = null)
    {
        IQueryable<User> query = _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.Id != 1);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lower = searchTerm.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(lower) ||
                u.Email.ToLower().Contains(lower) ||
                u.Role.ToLower().Contains(lower)
            );
        }

        return await query.ToListAsync();
    }
    
    public async Task<User?> GetByIdUser(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }
    
    public async Task<bool> IsUser(int id)
    {
        var isUser = await _context.Users.AnyAsync(u => u.Id == id);
        return isUser;
    }

    public async Task<bool> UpdatePassword(int userId, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user == null)
            return false;
        
        user.PasswordHash = newPassword;
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false; 
        }
    }
    
    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        user.IsDeleted = true;
        
        _context.Users.Update(user);
            
        var result = await _context.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> UpdateUserStatusAsync(int userId)
    {
        var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.IsDeleted = !user.IsDeleted;
        
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
    

}
