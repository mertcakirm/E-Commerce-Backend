using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories;

public class UserAddressRepository : IUserAddressRepository
{
    private readonly AppDbContext _context;
    public UserAddressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateUserAddressAsync(UserAddress userAddress)
    {
        if (userAddress == null)
            return false;

        await _context.UserAddresses.AddAsync(userAddress);
        var result = await _context.SaveChangesAsync();

        return result > 0;
    }

    public async Task<IEnumerable<UserAddress>> GetUserAddressAll(int userId)
    {
        return await _context.UserAddresses
            .Where(u => u.UserId == userId && u.IsDeleted == false)
            .ToListAsync();
    }

    public async Task<bool> DeleteUserAddressAsync(int addressId)
    {
        var address = await _context.UserAddresses.FirstOrDefaultAsync(u => u.Id == addressId);
        if (address == null)
            return false;

        address.IsDeleted = true;
        _context.UserAddresses.Update(address);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateUserAddressAsync(UserAddress userAddress, int addressId)
    {
        if (userAddress == null)
            return false;

        var existingAddress = await _context.UserAddresses.FirstOrDefaultAsync(u => u.Id == addressId);

        if (existingAddress == null)
            return false;

        existingAddress.AddressLine = userAddress.AddressLine;
        existingAddress.City = userAddress.City;
        existingAddress.PostalCode = userAddress.PostalCode;
        existingAddress.Country = userAddress.Country;
        existingAddress.PhoneNumber = userAddress.PhoneNumber;

        _context.UserAddresses.Update(existingAddress);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<UserAddress?> GetByIdAsync(int addressId)
    {
        return await _context.UserAddresses
            .Include(u => u.User)
            .FirstOrDefaultAsync(u => u.Id == addressId);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}