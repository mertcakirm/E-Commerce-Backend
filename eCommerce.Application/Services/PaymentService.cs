using System.Net;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Application.DTOs;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly UserValidator _userValidator;
        private readonly IPaymentRepository _paymentRepository;
        

        public PaymentService(UserValidator userValidator, IPaymentRepository paymentRepository)
        {
            _userValidator = userValidator;
            _paymentRepository = paymentRepository;
        }
        
        
        public async Task<ServiceResult<PagedResult<PaymentRecord>>> GetAllPaymentRecordsAsync(
            string token, int pageNumber, int pageSize)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);

            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<PagedResult<PaymentRecord>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var allRecords = await _paymentRepository.GetPaymentRecordsAsync();

            if (allRecords == null || !allRecords.Any())
                return ServiceResult<PagedResult<PaymentRecord>>.Fail("Kayıt bulunamadı.", HttpStatusCode.NotFound);

            var totalCount = allRecords.Count;
            var items = allRecords
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedResult = new PagedResult<PaymentRecord>(items, totalCount, pageNumber, pageSize);

            return ServiceResult<PagedResult<PaymentRecord>>.Success(pagedResult);
        }

        public async Task<ServiceResult<List<MonthlySalesReportDto>>> GetMonthlySalesReportAsync(DateTime startDate, DateTime endDate,string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
        
            if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult<List<MonthlySalesReportDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
            
            var payments = await _paymentRepository.GetPaymentsByDateRangeAsync(startDate, endDate);

            var report = payments
                .GroupBy(p => new { p.Order.OrderDate.Year, p.Order.OrderDate.Month })
                .Select(g => new MonthlySalesReportDto
                {
                    ReportMonth = $"{g.Key.Month:D2}/{g.Key.Year}",
                    transferTotal = g
                        .Where(x => x.PaymentMethod == "Havale")
                        .Sum(x => x.Order.OrderItems.Sum(oi => oi.Price * oi.Quantity)),
                    CreditCartTotal = g
                        .Where(x => x.PaymentMethod == "Credit_Card")
                        .Sum(x => x.Order.OrderItems.Sum(oi => oi.Price * oi.Quantity)),
                    TotalAmount = g.Sum(x => x.Order.OrderItems.Sum(oi => oi.Price * oi.Quantity)),
                    NetProfit = g.Sum(x => x.Order.OrderItems.Sum(oi => oi.Price * oi.Quantity)) * 0.1m,
                    OrdersCount = g.Count()
                })
                .OrderBy(r => r.ReportMonth)
                .ToList();

            
            return ServiceResult<List<MonthlySalesReportDto>>.Success(report);
        }
        
        
        public async Task<ServiceResult<string>> CreatePaymentRecordAsync(MonthlySalesReportDto dto,string token)
        {
                var isAdmin = await _userValidator.IsAdminAsync(token);
            
                if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult<string>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
                
                if (dto == null)
                    return ServiceResult<string>.Fail("Geçersiz veri gönderildi.", HttpStatusCode.BadRequest);

                if (!DateTime.TryParse(dto.ReportMonth, out DateTime reportDate))
                    return ServiceResult<string>.Fail("Geçersiz tarih formatı.", HttpStatusCode.BadRequest);

                var paymentRecord = new PaymentRecord
                {
                    ReportMonth = reportDate,
                    TransferTotal = dto.transferTotal,
                    CreditCartTotal = dto.CreditCartTotal,
                    TotalAmount = dto.TotalAmount,
                    NetProfit = dto.NetProfit,
                    OrdersCount = dto.OrdersCount
                };

                await _paymentRepository.AddPaymentRecordAsync(paymentRecord);

                return ServiceResult<string>.Success("Ödeme kaydı başarıyla oluşturuldu.");
        }
        
        
        
    }
}