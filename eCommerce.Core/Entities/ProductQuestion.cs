using System.Text.Json.Serialization;

namespace eCommerce.Core.Entities;

public class ProductQuestion : BaseEntity
{
    public int UserId { get; set; } 
    public User User { get; set; }

    public bool IsCorrect { get; set; }
    public string QuestionText { get; set; }


    public ICollection<ProductAnswer> Answers { get; set; }
    
    public int ProductId { get; set; }
    [JsonIgnore]
    public Product Product { get; set; }
}