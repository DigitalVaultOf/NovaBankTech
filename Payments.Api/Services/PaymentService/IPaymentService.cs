using Payments.Api.DTOS;
using Payments.Api.Models;

namespace Payments.Api.Services.PaymentService;

public interface IPaymentService
{
    Task<ResponseModel<MonthPaymentModel>> GetMonthPaymentsAsync(MonthPaymentDto dto);
    Task<ResponseModel<bool>> AddPaymentAsync(AddPaymentDto dto);
    Task<ResponseModel<bool>> MarkAsPaidAsync(PaySlipDto  dto);
    Task<ResponseModel<List<MonthPaymentModel>>> GetPaymentsHistoryAsync(Guid userId);
    Task<ResponseModel<bool>> GenerateMonthlyPaymentAsync(Guid userId);
}