using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IMessageRepository
{
    Task<bool> CreateMessageAsync(Message message);
    Task<List<Message>> GetAllMessagesAsync();
    Task<bool> ToggleMessageReply(int messageId, string answer);
}