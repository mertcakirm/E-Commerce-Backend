using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
    Task<bool> ClearAuditLogsHistoryAsync();
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetNotSeenAllAsync(int pageNumber, int pageSize);
    Task<bool> ToggleSeeLogAsync(int id);
}