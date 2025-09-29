using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;

    public AuthController(IAuthRepository authRepository, ITokenService tokenService)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        if (await _authRepository.UserExists(request.Email)) return BadRequest("Bu email zaten kayıtlı!");

        if (request.Password != request.ConfirmPassword) return BadRequest("Parolalar eşleşmiyor!");

        var newUser = new User
        {
            Name = request.UserName,
            Email = request.Email,
            AcceptEmails = request.AcceptEmails,
            PhoneNumber = request.PhoneNumber,
            Role = "User"
        };

        var createdUser = await _authRepository.Register(newUser, request.Password);

        return Ok(new
        {
            createdUser.Id,
            createdUser.Email,
            createdUser.Name
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var user = await _authRepository.Login(request.Email, request.Password);
        if (user == null) return Unauthorized("Email veya parola hatalı!");

        var tokenString = _tokenService.CreateToken(user);

        return Ok(tokenString);
    }
        
}