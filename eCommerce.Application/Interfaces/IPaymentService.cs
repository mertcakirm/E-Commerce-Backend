using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IPaymentService
{
    Task<ServiceResult<PagedResult<PaymentRecord>>> GetAllPaymentRecordsAsync(string token, int pageNumber, int pageSize);
    Task<ServiceResult<List<MonthlySalesReportDto>>> GetMonthlySalesReportAsync(DateTime startDate, DateTime endDate, string token);
    Task<ServiceResult<string>> CreatePaymentRecordAsync(MonthlySalesReportDto dto,string token);
}