using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(int? userId, string action, string entityName, int? entityId, string details);
    Task<IEnumerable<AuditLog>> GetAllAsync();
}