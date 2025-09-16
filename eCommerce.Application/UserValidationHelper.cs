using System.Net;
using System.Threading.Tasks;
using eCommerce.Application;
using eCommerce.Core.Entities;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Interfaces;

public class UserValidator
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepo;

    public UserValidator(ITokenService tokenService, IUserRepository userRepo)
    {
        _tokenService = tokenService;
        _userRepo = userRepo;
    }

    public async Task<ServiceResult<User>> ValidateAsync(string token)
    {
        var userId = _tokenService.GetUserIdFromToken(token);

        if (userId == null || userId <= 0)
        {
            return ServiceResult<User>.Fail(
                "Token geçersiz veya yetkisiz",
                HttpStatusCode.Unauthorized
            );
        }

        var user = await _userRepo.GetByIdUser(userId);
        if (user == null)
        {
            return ServiceResult<User>.Fail(
                "Kullanıcı bulunamadı",
                HttpStatusCode.NotFound
            );
        }

        return ServiceResult<User>.Success(user);
    }

    public async Task<ServiceResult<bool>> IsAdminAsync(string token)
    {
        var validation = await ValidateAsync(token);
        if (validation.IsFail)
            return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);

        var user = validation.Data!;
        bool isAdmin = user.Role == "Admin";

        return ServiceResult<bool>.Success(isAdmin);
    }

    public async Task<ServiceResult<bool>> HasRoleAsync(string token, string roleName)
    {
        var validation = await ValidateAsync(token);
        if (validation.IsFail)
            return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);

        var user = validation.Data!;
        bool hasRole = user.Role == roleName;

        return ServiceResult<bool>.Success(hasRole);
    }
}