using Payments.Api.Models;

namespace Payments.Api.Repositories.PaymentRepository;

public interface IPaymentRepository
{
    Task<MonthPaymentModel?> GetMonthPaymentsAsync(Guid userId, int month, int year);
    Task<MonthPaymentModel?> GetMonthPaymentsByIdAsync(Guid paymentId);
    Task AddPayment(MonthPaymentModel paymentModel);
    Task<List<MonthPaymentModel>> GetPaymentsHistoryAsync(Guid userId);

    Task<MonthPaymentModel?> GetBankSlipByNumberAsync(long bankSlipNumber);
    Task<List<MonthPaymentModel>> GetPendingBankSlipsAsync(Guid userId);
    Task<List<MonthPaymentModel>> GetPaidBankSlipsAsync(Guid userId);
}