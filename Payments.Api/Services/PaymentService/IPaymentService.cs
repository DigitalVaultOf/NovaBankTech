using Payments.Api.DTOS;
using Payments.Api.Models;

namespace Payments.Api.Services.PaymentService;

public interface IPaymentService
{
        Task<ResponseModel<MonthPaymentModel>> GetMonthPaymentAsync(MonthPaymentDto monthPaymentDto);
    
}