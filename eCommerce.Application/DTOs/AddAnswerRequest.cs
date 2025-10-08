namespace eCommerce.Application.DTOs;

public class AddAnswerRequest
{
    public int QuestionId { get; set; }
    public string AnswerText { get; set; }
}