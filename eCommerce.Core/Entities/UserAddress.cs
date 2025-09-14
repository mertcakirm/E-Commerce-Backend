using System.ComponentModel.DataAnnotations;

namespace eCommerce.Core.Entities;

public class UserAddress : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }

    public string AddressLine { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
    
    [Required]
    [MaxLength(15)]
    [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string PhoneNumber { get; set; }
}