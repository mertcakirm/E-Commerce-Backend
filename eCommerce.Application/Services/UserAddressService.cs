using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services;

public class UserAddressService : IUserAddressService
{
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly ITokenService _tokenService;

    public UserAddressService(IUserAddressRepository userAddressRepository, ITokenService tokenService)
    {
        _userAddressRepository = userAddressRepository;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<UserAddressDto>> CreateUserAddressAsync(UserAddressDto userAddressDto, string token)
    {
        var userId = _tokenService.GetUserIdFromToken(token);
        if (userId == null)
            return ServiceResult<UserAddressDto>.Fail("Kullanıcı bulunamadı", HttpStatusCode.NotFound);

        var newAddress = new UserAddress
        {
            UserId = userId,
            AddressLine = userAddressDto.AddressLine,
            City = userAddressDto.City,
            Country = userAddressDto.Country,
            PostalCode = userAddressDto.PostalCode,
            PhoneNumber = userAddressDto.PhoneNumber
        };

        var success = await _userAddressRepository.CreateUserAddressAsync(newAddress);
        if (!success)
            return ServiceResult<UserAddressDto>.Fail("Adres oluşturulamadı", HttpStatusCode.BadRequest);

        var resultDto = new UserAddressDto
        {
            AddressLine = newAddress.AddressLine,
            City = newAddress.City,
            Country = newAddress.Country,
            PostalCode = newAddress.PostalCode,
            PhoneNumber = newAddress.PhoneNumber
        };

        return ServiceResult<UserAddressDto>.Success(resultDto, status:HttpStatusCode.Created);
    }

    public async Task<ServiceResult<IEnumerable<UserAddressDto>>> GetUserAddressesAsync(string token)
    {
        var userId = _tokenService.GetUserIdFromToken(token);
        if (userId == null)
            return ServiceResult<IEnumerable<UserAddressDto>>.Fail("Kullanıcı bulunamadı", HttpStatusCode.NotFound);

        var addresses = await _userAddressRepository.GetUserAddressAll(userId);

        var dtoList = addresses.Select(a => new UserAddressDto
        {
            AddressLine = a.AddressLine,
            City = a.City,
            Country = a.Country,
            PostalCode = a.PostalCode,
            PhoneNumber = a.PhoneNumber
        });

        return ServiceResult<IEnumerable<UserAddressDto>>.Success(dtoList, status:HttpStatusCode.OK);
    }

    public async Task<ServiceResult<UserAddressDto>> UpdateUserAddressAsync(int addressId, UserAddressDto userAddressDto)
    {
        var existingAddress = await _userAddressRepository.GetByIdAsync(addressId);
        if (existingAddress == null)
            return ServiceResult<UserAddressDto>.Fail("Adres bulunamadı", HttpStatusCode.NotFound);

        existingAddress.AddressLine = userAddressDto.AddressLine;
        existingAddress.City = userAddressDto.City;
        existingAddress.Country = userAddressDto.Country;
        existingAddress.PostalCode = userAddressDto.PostalCode;
        existingAddress.PhoneNumber = userAddressDto.PhoneNumber;

        var success = await _userAddressRepository.UpdateUserAddressAsync(existingAddress, addressId);
        if (!success)
            return ServiceResult<UserAddressDto>.Fail("Adres güncellenemedi", HttpStatusCode.BadRequest);

        var resultDto = new UserAddressDto
        {
            AddressLine = existingAddress.AddressLine,
            City = existingAddress.City,
            Country = existingAddress.Country,
            PostalCode = existingAddress.PostalCode,
            PhoneNumber = existingAddress.PhoneNumber
        };

        return ServiceResult<UserAddressDto>.Success(resultDto, status:HttpStatusCode.OK);
    }

    public async Task<ServiceResult<bool>> DeleteUserAddressAsync(int addressId)
    {
        var existingAddress = await _userAddressRepository.GetByIdAsync(addressId);
        if (existingAddress == null)
            return ServiceResult<bool>.Fail("Adres bulunamadı", HttpStatusCode.NotFound);

        var success = await _userAddressRepository.DeleteUserAddressAsync(addressId);
        if (!success)
            return ServiceResult<bool>.Fail("Adres silinemedi", HttpStatusCode.BadRequest);

        return ServiceResult<bool>.Success(true, status:HttpStatusCode.OK);
    }
}