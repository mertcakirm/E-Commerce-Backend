namespace eCommerce.Core.Entities;

public class User : BaseEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string PhoneNumber { get; set; }
    public bool AcceptEmails { get; set; }
    public string Role { get; set; } = "User"; 

    public ICollection<Order> Orders { get; set; }
    public Cart Cart { get; set; }
    public ICollection<UserAddress> Addresses { get; set; }
    public ICollection<Wishlist> Wishlists { get; set; }
}