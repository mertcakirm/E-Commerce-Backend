using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using eCommerce.Core.Helpers;
using Microsoft.EntityFrameworkCore;
namespace eCommerce.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;

    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> Register(User user, string password)
    {
        user.PasswordHash = CreatePassword.CreatePasswordHash(password);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null) return null;

        if (!CreatePassword.VerifyPasswordHash(password, user.PasswordHash)) return null;

        return user;
    }
    
    
    public async Task<User> LoginAdmin(string email, string password)
    {
        const string adminEmail = "test";
        
        const string adminPasswordHash = "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08"; 

        if (email != adminEmail) return null;

        if (!CreatePassword.VerifyPasswordHash(password, adminPasswordHash)) return null;

        var adminUser = new User
        {
            Id = 100,
            Email = adminEmail,
            Name = "Admin",
            Role = "Admin"
        };

        return await Task.FromResult(adminUser);
    }
    

    public async Task<bool> UserExists(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email);
    }
}