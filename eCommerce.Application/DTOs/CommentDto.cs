namespace eCommerce.Application.DTOs;


public class CommentCreateDto
{
    public int ProductId { get; set; }
    public string CommentText { get; set; }
    public int Rating { get; set; }
}
