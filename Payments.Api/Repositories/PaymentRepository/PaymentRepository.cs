using Microsoft.EntityFrameworkCore;
using Payments.Api.Data;
using Payments.Api.DTOS;
using Payments.Api.Models;

namespace Payments.Api.Repositories.PaymentRepository;

public class PaymentRepository(AppDbContext context) : IPaymentRepository
{
    public async Task<MonthPaymentModel?> GetMonthPaymentsAsync(Guid userId, int month, int year)
    {
        return await context.MonthPayments.FirstOrDefaultAsync(p =>
            p.UserId == userId && p.Month == month && p.Year == year);
    }

    public async Task<MonthPaymentModel?> GetMonthPaymentsByIdAsync(Guid paymentId)
    {
        return await context.MonthPayments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
    }

    public Task AddPayment(MonthPaymentModel paymentModel)
    {
        context.MonthPayments.Add(paymentModel);
        return Task.CompletedTask;
    }

    public async Task<List<MonthPaymentModel>> GetPaymentsHistoryAsync(Guid userId)
    {
        return await context.MonthPayments.Where(p => p.UserId == userId).ToListAsync();
    }

    public async Task<MonthPaymentModel?> GetBankSlipByNumberAsync(long bankSlipNumber)
    {
        return await context.MonthPayments
            .FirstOrDefaultAsync(p => p.BankSlipNumber == bankSlipNumber);
    }

    public async Task<List<MonthPaymentModel>> GetPendingBankSlipsAsync(Guid userId)
    {
        return await context.MonthPayments
            .Where(p => p.UserId == userId && !p.IsPaid)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MonthPaymentModel>> GetPaidBankSlipsAsync(Guid userId)
    {
        return await context.MonthPayments
            .Where(p => p.UserId == userId && p.IsPaid)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
}