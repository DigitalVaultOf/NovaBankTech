using Microsoft.EntityFrameworkCore;
using Payments.Api.Data;
using Payments.Api.Models;

namespace Payments.Api.Repositories.PaymentRepository;

public class PaymentRepository(PaymentDbContext context) : IPaymentRepository
{
    private readonly PaymentDbContext _context = context;

    public async Task<MonthPaymentModel?> GetMonthPaymentAsync(Guid userId, int month, int year)
    {
        return await  _context.MonthPayments.FirstOrDefaultAsync(p => p.UserId == userId && p.Month == month && p.Year == year);
    }
}