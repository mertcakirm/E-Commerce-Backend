using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentRecord>> GetPaymentRecordsAsync()
    {
        return await _context.PaymentRecords
            .OrderByDescending(p => p.ReportMonth)
            .ToListAsync();
    }
    
    public async Task<List<Payment?>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.OrderItems)
            .Where(p => p.Order.OrderDate >= startDate && p.Order.OrderDate <= endDate)
            .ToListAsync();
    }
    
    public async Task AddPaymentRecordAsync(PaymentRecord paymentRecord)
    {
        await _context.PaymentRecords.AddAsync(paymentRecord);
        await _context.SaveChangesAsync();
    }
    
}