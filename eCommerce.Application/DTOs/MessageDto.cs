using eCommerce.Core.Entities;

namespace eCommerce.Application.DTOs;

public class MessageDto
{
    public string MessageTitle { get; set; }
    public string MessageText { get; set; }
}

public class MessageResponseDto
{
    public int Id { get; set; }
    public string UserEmail { get; set; }
    public string MessageTitle { get; set; }
    public string MessageText { get; set; }
    public bool IsReply { get; set; }
    public string Answer {get; set;}
}