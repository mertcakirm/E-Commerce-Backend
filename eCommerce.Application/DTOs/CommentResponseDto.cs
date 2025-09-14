namespace eCommerce.Application.DTOs;

public class CommentListDto
{
    public int Id { get; set; }
    public string CommentText { get; set; }
    public int Rating { get; set; }
    public string UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}