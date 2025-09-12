using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsers();
    Task<User?> GetByIdUser(int userId);
    Task<bool> IsUser(int id);
    Task<bool> DeleteUserAsync(int userId);
    Task<bool> UpdatePassword(int userId, string newPassword);
}