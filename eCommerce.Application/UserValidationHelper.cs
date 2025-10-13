using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
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
        if (string.IsNullOrWhiteSpace(token))
            return ServiceResult<bool>.Fail("Token bulunamadı.", HttpStatusCode.Unauthorized);

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token.Replace("Bearer ", ""));

            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                            ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (string.IsNullOrEmpty(roleClaim))
                return ServiceResult<bool>.Fail("Role bilgisi bulunamadı.", HttpStatusCode.Forbidden);

            bool isAdmin = roleClaim.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            return ServiceResult<bool>.Success(isAdmin);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Token çözümlenemedi: {ex.Message}", HttpStatusCode.Unauthorized);
        }
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