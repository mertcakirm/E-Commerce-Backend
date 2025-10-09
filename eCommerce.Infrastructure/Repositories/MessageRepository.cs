using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateMessageAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
        return true;
    }

    public async Task<List<Message>> GetAllMessagesAsync()
    {
        return await _context.Messages.Include(m=>m.User).ToListAsync();
    }

    public async Task<bool> ToggleMessageReply(int messageId,string answer)
    {
        var message = await _context.Messages.SingleOrDefaultAsync(m => m.Id == messageId);
        if (message == null) return false;
        message.IsReply = true;
        message.Answer = answer;
        await _context.SaveChangesAsync();
        return true;;
    }
    
    
    
    

}