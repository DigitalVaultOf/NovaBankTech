using Payments.Api.DTOS;
using Payments.Api.Models;

namespace Payments.Api.Repositories.PaymentRepository;

public interface IPaymentRepository
{
    Task<MonthPaymentModel?> GetMonthPaymentAsync(Guid userId, int month, int year);
}