using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IMessageService
{
    Task<ServiceResult<bool>> CreateMessageAsync(MessageDto message, string token);
    Task<ServiceResult<PagedResult<MessageResponseDto>>> GetAllMessagesAsync(int pageNumber, int pageSize, string token);
    Task<ServiceResult<bool>> ToggleMessageReplyAsync(int messageId, string answer,string token);
    Task<ServiceResult<bool>> RemoveMessageAsync(int messageId, string token);
}