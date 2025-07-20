using Payments.Api.DTOS;
using Payments.Api.Models;

namespace Payments.Api.Services.PaymentService;

public interface IPaymentService
{
    Task<ResponseModel<MonthPaymentModel>> GetMonthPaymentsAsync(MonthPaymentDto dto);
    Task<ResponseModel<bool>> AddPaymentAsync(AddPaymentDto dto);
    Task<ResponseModel<bool>> MarkAsPaidAsync(PayBankSlipDto dto);
    Task<ResponseModel<List<MonthPaymentModel>>> GetPaymentsHistoryAsync(Guid userId);
    Task<ResponseModel<bool>> GenerateMonthlyPaymentAsync(Guid userId);


    Task<ResponseModel<string>> GenerateBankSlipAsync(GenerateBankSlipDto dto); // ← string, não long
    Task<ResponseModel<ValidateBankSlipResponseDto>> ValidateBankSlipAsync(string bankSlipNumber); // ← string, não long
    Task<ResponseModel<List<BoletoListItemDto>>> GetPendingBankSlipsAsync();
    Task<ResponseModel<List<BoletoListItemDto>>> GetPaidBankSlipsAsync();
    Task<ResponseModel<bool>> PayBankSlipAsync(PayBankSlipDto dto);

    Task<ResponseModel<PayBankSlipResponseDto>> PayPartialBankSlipAsync(PayPartialBankSlipDto dto);
}