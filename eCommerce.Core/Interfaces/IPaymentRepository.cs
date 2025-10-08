using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IPaymentRepository
{
    Task<List<PaymentRecord>> GetPaymentRecordsAsync();
    Task<List<Payment?>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddPaymentRecordAsync(PaymentRecord paymentRecord);
    Task<bool> RemovePaymentRecordAsync(int recordId);
}