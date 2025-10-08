using eCommerce.Core.Entities;

namespace eCommerce.Application.DTOs;

public class GetAllQuestionsDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }

    public string UserName { get; set; }
    public string QuestionText { get; set; }
    public DateTime CreatedDate { get; set; }
}