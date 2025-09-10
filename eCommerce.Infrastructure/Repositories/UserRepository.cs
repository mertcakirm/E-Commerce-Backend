using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories;

public class UserRepository: IUserRepository
{
        private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> IsUser(int id)
    {
        var isUser = await _context.Users.AnyAsync(u => u.Id == id);
        return isUser;
    }
}