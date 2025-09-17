using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<IEnumerable<AuditLog>> GetAllAsync();
}