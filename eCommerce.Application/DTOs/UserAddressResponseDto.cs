namespace eCommerce.Application.DTOs;

public class UserAddressResponseDto
{
    public int Id { get; set; }
    public string AddressTitle {get; set;}
    public string AddressLine { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
}