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
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetUserById()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var result = await _userService.GetUserByIdAsync(token);

        if (result.IsFail)
            return StatusCode((int)result.Status, result.ErrorMessage);

        return Ok(result.Data);
    }
    
    
    [Authorize]
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllUsers(
        [FromHeader(Name = "Authorization")] string token,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null) 
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var users = await _userService.GetAllUsers(token, pageNumber, pageSize, searchTerm);
        return Ok(users);
    }
    
    [Authorize]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(int userId,[FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token missing");

        var result = await _userService.DeleteUser(token, userId);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);
    }
    
    [Authorize]
    [HttpPut("status/{userId}")]
    public async Task<IActionResult> DeleteUser([FromHeader(Name = "Authorization")] string token,int userId)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token missing");

        var result = await _userService.UpdateUserStatus(userId,token);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);
    }

}