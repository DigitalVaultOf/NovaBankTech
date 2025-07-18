using Payments.Api.Models;

namespace Payments.Api.Repositories.PaymentRepository;

public interface IPaymentRepository
{
    Task<MonthPaymentModel?> GetMonthPaymentsAsync(Guid userId, int month, int year);
    Task<MonthPaymentModel?> GetMonthPaymentsByIdAsync(Guid paymentId);
    Task AddPayment(MonthPaymentModel paymentModel);
    Task<List<MonthPaymentModel>> GetPaymentsHistoryAsync(Guid userId);
}