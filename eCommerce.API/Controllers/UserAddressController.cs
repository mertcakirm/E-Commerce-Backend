using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;

namespace eCommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserAddressController : ControllerBase
    {
        private readonly IUserAddressService _userAddressService;

        public UserAddressController(IUserAddressService userAddressService)
        {
            _userAddressService = userAddressService;
        }

        // GET: api/UserAddress
        [HttpGet]
        public async Task<IActionResult> GetAddresses([FromHeader(Name = "Authorization")] string token)
        {
            var result = await _userAddressService.GetUserAddressesAsync(token);

            if (!result.IsSuccess)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(result.Data);
        }

        // POST: api/UserAddress
        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromHeader(Name = "Authorization")] string token,[FromBody] UserAddressDto userAddressDto)
        {
            var result = await _userAddressService.CreateUserAddressAsync(userAddressDto, token);

            if (!result.IsSuccess)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return StatusCode((int)result.Status, result.Data);
        }

        // PUT: api/UserAddress/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress([FromHeader(Name = "Authorization")] string token,int id, [FromBody] UserAddressDto userAddressDto)
        {
            var result = await _userAddressService.UpdateUserAddressAsync(id, userAddressDto,token);

            if (!result.IsSuccess)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(result.Data);
        }

        // DELETE: api/UserAddress/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress([FromHeader(Name = "Authorization")] string token,int id)
        {
            var result = await _userAddressService.DeleteUserAddressAsync(id,token);

            if (!result.IsSuccess)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(new { Message = "Adres başarıyla silindi." });
        }
    }
}