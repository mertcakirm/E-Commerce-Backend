using System.ComponentModel.DataAnnotations;

namespace eCommerce.Core.Entities;

public class UserAddress : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }

    public string AddressTitle { get; set; }
    public string AddressLine { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    
}