using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
    int GetUserIdFromToken(string token);
    int GetRoleIdFromToken(string token);
}