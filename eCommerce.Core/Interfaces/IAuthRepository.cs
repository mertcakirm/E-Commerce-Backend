using eCommerce.Core.Entities;
namespace eCommerce.Core.Interfaces;

public interface IAuthRepository
{
    Task<User> Register(User user, string password);
    Task<User> Login(string email, string password);
    Task<bool> UserExists(string email);
    Task<User> LoginAdmin(string email, string password);
}