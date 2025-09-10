using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace eCommerce.Application.Services;

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepository;
        

        public TokenService(IConfiguration config, IUserRepository userRepository)
        {
            _config = config;
            _userRepository = userRepository;
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Name),
                new Claim("roleId", user.Role)
            };

            // SHA256 kullanıyoruz
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public int GetUserIdFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token boş olamaz");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token.Replace("Bearer ", ""));

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("Token içinde userId bulunamadı");

            return int.Parse(userIdClaim.Value);
        }
        
        public int GetRoleFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token boş olamaz");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token.Replace("Bearer ", ""));

            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");

            if (roleClaim == null)
                throw new UnauthorizedAccessException("Token içinde roleId bulunamadı");

            return int.Parse(roleClaim.Value);
        }

        public async Task<bool> IsUserAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            int userId;
            try
            {
                userId = GetUserIdFromToken(token);
            }
            catch
            {
                return false;
            }

            var exists = await _userRepository.IsUser(userId);

            return exists;
        }
    }
