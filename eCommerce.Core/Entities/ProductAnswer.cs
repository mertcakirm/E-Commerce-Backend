namespace eCommerce.Core.Entities;

public class ProductAnswer : BaseEntity
{
    public int QuestionId { get; set; }
    public ProductQuestion Question { get; set; }

    public int UserId { get; set; } 
    public User User { get; set; }

    public string AnswerText { get; set; }

}