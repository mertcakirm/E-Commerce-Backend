using eCommerce.Application;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController: ControllerBase
{
    private readonly IUserService  _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllUsers([FromHeader(Name = "Authorization")] string token,[FromQuery] int pageNumber=1, [FromQuery] int pageSize=10)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var users = await _userService.GetAllUsers(token,pageNumber,pageSize);
        return Ok(users);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromHeader(Name = "Authorization")] string token,
        [FromQuery] string oldPassword,
        [FromQuery] string newPassword)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");
        if (string.IsNullOrEmpty(oldPassword))
            return BadRequest("Eski şifre gerekli");
        if (string.IsNullOrEmpty(newPassword))
            return BadRequest("Yeni şifre gerekli");

        var result = await _userService.UpdatePassword(token, oldPassword, newPassword);

        if (result.IsFail)
            return StatusCode((int)result.Status, result.ErrorMessage);

        return Ok(new 
        {
            Success = true,
            Message = "Şifre başarıyla değiştirildi",
        });
    }

}