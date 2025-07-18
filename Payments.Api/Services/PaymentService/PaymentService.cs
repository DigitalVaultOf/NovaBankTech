using Payments.Api.Clients;
using Payments.Api.Data;
using Payments.Api.DTOS;
using Payments.Api.Models;
using Payments.Api.Repositories.PaymentRepository;
using Payments.Api.Utils;

namespace Payments.Api.Services.PaymentService;

public class PaymentService(
    IPaymentRepository paymentRepository,
    AppDbContext context,
    BankApiClient bankApiClient,
    IHttpContextAccessor httpContextAccessor)
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

            var bankSlipNumber = BankSlipNumberGenerator.Generate();

            var newPayment = new MonthPaymentModel
            {
                UserId = dto.UserId,
                BankSlipNumber = bankSlipNumber,
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

    public async Task<ResponseModel<bool>> MarkAsPaidAsync(PaySlipDto dto)
    {
        var response = new ResponseModel<bool>();

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var existingPayment = await paymentRepository.GetMonthPaymentsByIdAsync(dto.PaymentId);

            if (existingPayment is null)
            {
                response.Message = "Nenhum boleto pendente foi encontrado.";
                response.Data = false;
                return response;
            }

            if (existingPayment.IsPaid)
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
                Value = existingPayment.Amount,
                UserPassword = dto.UserPassword
            };

            var debitSuccess = await bankApiClient.DebitFromAccountAsync(debitDto);

            if (!debitSuccess)
            {
                throw new Exception("A API de Banco recusou a operação de débito.");
            }

            existingPayment.IsPaid = true;
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

    public async Task<ResponseModel<List<MonthPaymentModel>>> GetPaymentsHistoryAsync(Guid userId)
    {
        var response = new ResponseModel<List<MonthPaymentModel>>();

        try
        {
            var paymentHistory = await paymentRepository.GetPaymentsHistoryAsync(userId);

            if (paymentHistory.Count is 0)
            {
                response.Message = "Nenhum histórico de pagamento foi encontrado para este usuário.";
                response.Data = []; // Retorna uma lista vazia para o frontend
                return response;
            }

            response.Data = paymentHistory;
            response.Message = "Histórico de pagamentos encontrado com sucesso.";
        }
        catch (Exception ex)
        {
            response.Message = "Ocorreu um erro ao buscar o histórico de pagamentos.";
            Console.WriteLine($"LOG ERROR: Falha em GetPaymentsHistoryAsync: {ex}");
        }

        return response;
    }

    public async Task<ResponseModel<bool>> GenerateMonthlyPaymentAsync(Guid userId)
    {
        var response = new ResponseModel<bool>();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var (month, year) = (DateTime.UtcNow.Month, DateTime.UtcNow.Year);

            var existingPayment = await paymentRepository.GetMonthPaymentsAsync(userId, month, year);

            if (existingPayment is not null)
            {
                response.Message = "Uma cobrança para este mês já foi gerada para o usuário.";
                response.Data = false;
                return response;
            }

            var accountNumber = httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;

            if (string.IsNullOrEmpty(accountNumber))
            {
                throw new Exception("Não foi possível identificar o número da conta do usuário a partir do token.");
            }

            var bankSlipNumber = BankSlipNumberGenerator.Generate();


            var newPayment = new MonthPaymentModel
            {
                UserId = userId,
                AccountNumber = accountNumber,
                BankSlipNumber = bankSlipNumber,
                Amount = 1500.00m,
                Month = month,
                Year = year,
                DueDate = new DateTime(year, month, 10), // Exemplo: Vencimento todos os dias 10 do mês.
                IsPaid = false // Começa como "não pago"
            };

            await paymentRepository.AddPayment(newPayment);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Data = true;
            response.Message = "Cobrança mensal gerada com sucesso!";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Message = "Ocorreu um erro ao gerar a cobrança mensal.";
            response.Data = false;
            Console.WriteLine($"LOG ERROR: Falha em GenerateMonthlyPaymentAsync: {ex}");
        }

        return response;
    }
}