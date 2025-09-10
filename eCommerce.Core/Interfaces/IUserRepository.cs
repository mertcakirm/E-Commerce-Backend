using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IUserRepository
{
    Task<bool> IsUser(int id);

}