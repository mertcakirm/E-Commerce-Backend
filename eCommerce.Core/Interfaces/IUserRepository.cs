using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsers();
    Task<bool> IsUser(int id);

}