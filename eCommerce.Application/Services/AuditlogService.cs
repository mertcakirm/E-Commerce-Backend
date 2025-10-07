using System.Net;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly UserValidator _userValidator;

    public AuditLogService(IAuditLogRepository auditRepo, UserValidator userValidator)
    {
        _auditRepo = auditRepo;
        _userValidator = userValidator;
    }

    public async Task LogAsync(int? userId, string action, string entityName, int? entityId, string details)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details
        };

        await _auditRepo.AddAsync(log);
    }

    public async Task<ServiceResult<PagedResult<AuditLog>>> GetAllAsync(string token, int pageNumber = 1, int pageSize = 10)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult<PagedResult<AuditLog>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var (items, totalCount) = await _auditRepo.GetAllAsync(pageNumber, pageSize);

        var pagedResult = new PagedResult<AuditLog>(items, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<AuditLog>>.Success(pagedResult, "Loglar başarıyla getirildi.");
    }
    
    public async Task<ServiceResult<PagedResult<AuditLog>>> GetNotSeenAllAsync(string token, int pageNumber = 1, int pageSize = 10)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult<PagedResult<AuditLog>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var (items, totalCount) = await _auditRepo.GetNotSeenAllAsync(pageNumber, pageSize);

        var pagedResult = new PagedResult<AuditLog>(items, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<AuditLog>>.Success(pagedResult, "Loglar başarıyla getirildi.");
    }

    public async Task<ServiceResult<bool>> ToggleSeeLogAsync(int id, string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult<bool>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var toggle = await _auditRepo.ToggleSeeLogAsync(id);
        if (toggle == null)
            return ServiceResult<bool>.Fail("İşlem bulunamadı!", HttpStatusCode.NotFound);

        return ServiceResult<bool>.Success(true, "Log durumu başarıyla değiştirildi.");
    }

    public async Task<ServiceResult<bool>> ClearAuditLogsHistoryAsync(string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult<bool>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        try
        {
            var result = await _auditRepo.ClearAuditLogsHistoryAsync();

            if (!result)
                return ServiceResult<bool>.Fail("Log temizleme işlemi başarısız oldu.", HttpStatusCode.InternalServerError);

            return ServiceResult<bool>.Success(true, "Tüm log geçmişi başarıyla temizlendi.");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Log temizleme sırasında hata oluştu: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}