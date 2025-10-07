using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(int? userId, string action, string entityName, int? entityId, string details);
    Task<ServiceResult<PagedResult<AuditLog>>> GetAllAsync(string token, int pageNumber = 1, int pageSize = 10);
    Task<ServiceResult<bool>> ClearAuditLogsHistoryAsync(string token);
}