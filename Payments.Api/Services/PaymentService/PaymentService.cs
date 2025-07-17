using Payments.Api.DTOS;
using Payments.Api.Models;
using Payments.Api.Repositories.PaymentRepository;

namespace Payments.Api.Services.PaymentService;

public class PaymentService(IPaymentRepository paymentRepository) : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;

    public async Task<ResponseModel<MonthPaymentModel>> GetMonthPaymentAsync(MonthPaymentDto dto)
    {
        var response = new ResponseModel<MonthPaymentModel>();

        try
        {
            var payment = await _paymentRepository.GetMonthPaymentAsync(dto.UserId, dto.Month, dto.Year);

            if (payment is null)
            {
                response.Message = "Pagamentos não encontrados.";
            }
            else
            {
                response.Data = payment;
                response.Message = "Pagamentos encontrados com sucesso!";
            }
        }
        catch (Exception ex)
        {
            response.Message = "Erro ao buscar pagamentos.";
            Console.WriteLine($"Erro ao buscar pagamentos: {ex}");
        }

        return response;
    }
}