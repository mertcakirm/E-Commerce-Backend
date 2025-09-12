namespace eCommerce.Core.Entities;

public class Comment : BaseEntity
{
    public int userId { get; set; }
    public int rating { get; set; }
    public string comment { get; set; }
}