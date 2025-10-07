using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetNotSeenAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.AuditLogs
            .Where(a => !a.IsDeleted && a.IsSeen == false)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    
    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.AuditLogs
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount); // tuple ile döndürüyoruz
    }

    public async Task<bool> ToggleSeeLogAsync(int id)
    {
        var query = await _context.AuditLogs.FirstOrDefaultAsync(a=>a.Id == id);
        if (query == null) return false;
        query.IsSeen = !query.IsSeen;
        await _context.SaveChangesAsync();
        return true;
        
    }

    public async Task<bool> ClearAuditLogsHistoryAsync()
    {
        try
        {
            _context.AuditLogs.RemoveRange(_context.AuditLogs);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    
}