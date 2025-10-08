namespace eCommerce.Application.DTOs;

public class AddQuestionRequest
{
    public int ProductId { get; set; }
    public string QuestionText { get; set; }
}