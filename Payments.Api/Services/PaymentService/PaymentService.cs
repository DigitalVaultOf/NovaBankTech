using Payments.Api.Clients;
using Payments.Api.Data;
using Payments.Api.DTOS;
using Payments.Api.Models;
using Payments.Api.Repositories.PaymentRepository;

namespace Payments.Api.Services.PaymentService;

public class PaymentService(IPaymentRepository paymentRepository, AppDbContext context, BankApiClient bankApiClient)
    : IPaymentService
{
    public async Task<ResponseModel<MonthPaymentModel>> GetMonthPaymentsAsync(MonthPaymentDto dto)
    {
        var response = new ResponseModel<MonthPaymentModel>();

        try
        {
            var payment = await paymentRepository.GetMonthPaymentsAsync(dto.UserId, dto.Month, dto.Year);

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
            Console.WriteLine($"LOG ERROR: Falha em GetMonthPaymentAsync: {ex}");
        }

        return response;
    }

    public async Task<ResponseModel<bool>> AddPaymentAsync(AddPaymentDto dto)
    {
        var response = new ResponseModel<bool>();

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var existingPayment = await paymentRepository.GetMonthPaymentsAsync(dto.UserId, dto.Month, dto.Year);

            if (existingPayment is not null)
            {
                response.Message = "Já existe um registro de pagamento para este usuário e período.";
                response.Data = false;
                return response;
            }

            var newPayment = new MonthPaymentModel
            {
                UserId = dto.UserId,
                AccountNumber = dto.AccountNumber,
                Amount = dto.Amount,
                Month = dto.Month,
                Year = dto.Year,
                DueDate = dto.DueDate
            };

            await paymentRepository.AddPayment(newPayment);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Message = "Pagamento adicionado com sucesso!";
            response.Data = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Message = "Ocorreu um erro ao adicionar o pagamento.";
            response.Data = false;
            Console.WriteLine($"LOG ERROR: Falha em AddPaymentAsync: {ex}");
        }

        return response;
    }

    public async Task<ResponseModel<bool>> MarkAsPaidAsync(Guid paymentId)
    {
        var response = new ResponseModel<bool>();

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var existingPayment = await paymentRepository.GetMonthPaymentsByIdAsync(paymentId);

            if (existingPayment is null)
            {
                response.Message = "Nenhum boleto pendente foi encontrado.";
                response.Data = false;
                return response;
            }

            if (existingPayment.BankSlipIsPaid)
            {
                response.Message = "Este boleto já foi pago anteriormente.";
                response.Data = false;
                return response;
            }

            var userBalance = await bankApiClient.GetBalanceAsync();

            if (userBalance < existingPayment.Amount || userBalance <= 0)
            {
                response.Message = "Saldo insuficiente para realizar o pagamento do boleto.";
                response.Data = false;
                return response;
            }

            var debitDto = new DebitPaymentDto
            {
                AccountNumber = existingPayment.AccountNumber,
                Value = existingPayment.Amount
            };
            
            var debitSuccess = await bankApiClient.DebitFromAccountAsync(debitDto);
            
            if (!debitSuccess)
            {
                throw new Exception("A API de Banco recusou a operação de débito.");
            }

            existingPayment.BankSlipIsPaid = true;
            existingPayment.PaymentDate = DateTime.Now;
            existingPayment.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Message = "Boleto marcado como concluído com sucesso!";
            response.Data = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Message = "Ocorreu um erro ao marcar o pagamento como concluído.";
            response.Data = false;
            Console.WriteLine($"LOG ERROR: Falha em MarkAsPaidAsync: {ex}");
        }

        return response;
    }
}