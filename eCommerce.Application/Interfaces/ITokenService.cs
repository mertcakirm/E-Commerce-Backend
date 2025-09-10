using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
    int GetUserIdFromToken(string token);
    string GetRoleFromToken(string token);
    Task<bool> IsUserAsync(string token);
}