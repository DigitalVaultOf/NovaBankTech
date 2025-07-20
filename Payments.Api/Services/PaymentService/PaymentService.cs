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
    GetUserData getUserData)
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

            var bankSlipNumber = BankSlipNumberManager.Generate();

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


    public async Task<ResponseModel<bool>> MarkAsPaidAsync(PayBankSlipDto dto)
    {
        var response = new ResponseModel<bool>();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var bankSlipNumberLong = BankSlipNumberManager.ConvertToLong(dto.BankSlipNumber);

            if (bankSlipNumberLong == null)
            {
                response.Message = "Número de boleto inválido.";
                response.Data = false;
                return response;
            }

            var existingPayment = await paymentRepository.GetBankSlipByNumberAsync(bankSlipNumberLong.Value);

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
                throw new Exception("A API do banco recusou a operação de débito.");
            }

            existingPayment.IsPaid = true;
            existingPayment.PaymentDate = DateTime.UtcNow;
            existingPayment.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Message = "Boleto marcado como pago com sucesso!";
            response.Data = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Message = "Ocorreu um erro ao marcar o boleto como pago.";
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
                response.Data = [];
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

            var accountNumber = getUserData.GetCurrentAccountNumber();

            var bankSlipNumber = BankSlipNumberManager.Generate();

            var newPayment = new MonthPaymentModel
            {
                UserId = userId,
                AccountNumber = accountNumber,
                BankSlipNumber = bankSlipNumber,
                Amount = 1500.00m,
                Month = month,
                Year = year,
                DueDate = new DateTime(year, month, 10).AddDays(30),
                IsPaid = false
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

    public async Task<ResponseModel<string>> GenerateBankSlipAsync(GenerateBankSlipDto dto)
    {
        var response = new ResponseModel<string>();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            switch (dto.Amount)
            {
                case <= 0:
                    response.Message = "O valor do boleto deve ser maior que zero.";
                    response.Data = null;
                    return response;
                case > 999999.99m:
                    response.Message = "O valor do boleto não pode exceder R$ 999.999,99.";
                    response.Data = null;
                    return response;
            }

            var userId = getUserData.GetCurrentUserId();
            var accountNumber = getUserData.GetCurrentAccountNumber();

            Console.WriteLine($"DEBUG: Starting boleto generation for UserId={userId}, AccountNumber={accountNumber}");

            var bankSlipNumber = BankSlipNumberManager.Generate();

            Console.WriteLine($"DEBUG: Generated bankSlipNumber = {bankSlipNumber}");

            var newBoleto = new MonthPaymentModel
            {
                UserId = userId,
                AccountNumber = accountNumber,
                BankSlipNumber = bankSlipNumber,
                Amount = dto.Amount,
                Description = dto.Description,
                Month = DateTime.UtcNow.Month,
                Year = DateTime.UtcNow.Year,
                DueDate = dto.DueDate ?? CustomDueDate.BrazilianDueDate(), // ✅ CORRIGIR CustomDueDate
                IsPaid = false
            };

            Console.WriteLine($"DEBUG: About to save boleto with BankSlipNumber = {newBoleto.BankSlipNumber}");

            await paymentRepository.AddPayment(newBoleto);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Data = BankSlipNumberManager.ConvertToString(bankSlipNumber);
            response.Message = "Boleto gerado com sucesso!";

            Console.WriteLine($"DEBUG: Boleto saved successfully. Returning: {response.Data}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            response.Message = "Erro ao gerar boleto.";
            Console.WriteLine($"DEBUG ERROR: Falha em GenerateBoletoAsync: {ex.Message}");
        }

        return response;
    }

    public async Task<ResponseModel<ValidateBankSlipResponseDto>>
        ValidateBankSlipAsync(string bankSlipNumber)
    {
        var response = new ResponseModel<ValidateBankSlipResponseDto>();

        try
        {
            Console.WriteLine($"DEBUG VALIDATE: Validating BankSlipNumber = {bankSlipNumber}");

            var bankSlipNumberLong = BankSlipNumberManager.ConvertToLong(bankSlipNumber);

            if (bankSlipNumberLong is null)
            {
                response.Message = "Número do boleto inválido.";
                return response;
            }

            var boleto = await paymentRepository.GetBankSlipByNumberAsync(bankSlipNumberLong.Value);

            if (boleto is null)
            {
                response.Message = "Boleto não encontrado.";
                return response;
            }

            response.Data = new ValidateBankSlipResponseDto
            {
                BankSlipNumber = BankSlipNumberManager.ConvertToString(boleto.BankSlipNumber!.Value),
                Amount = boleto.Amount,
                AccountNumber = boleto.AccountNumber,
                CreatedAt = boleto.CreatedAt,
                DueDate = boleto.DueDate,
                IsPaid = boleto.IsPaid,
                PaymentDate = boleto.PaymentDate,
                Description = boleto.Description,
                Customer = await getUserData.GetUserNameForBoletoAsync(boleto.AccountNumber)
            };

            response.Message = boleto.IsPaid ? "Boleto já foi pago." : "Boleto válido e pronto para ser pago.";
        }
        catch (Exception ex)
        {
            response.Message = "Erro ao validar boleto.";
            Console.WriteLine($"DEBUG ERROR: ValidateBoletoAsync failed: {ex.Message}");
        }

        return response;
    }

    public async Task<ResponseModel<List<BoletoListItemDto>>> GetPendingBankSlipsAsync()
    {
        var response = new ResponseModel<List<BoletoListItemDto>>();
        try
        {
            var userId = getUserData.GetCurrentUserId();
            var pendingBoletos = await paymentRepository.GetPendingBankSlipsAsync(userId);
            response.Data = [];

            foreach (var boleto in pendingBoletos)
            {
                var userName = await getUserData.GetUserNameForBoletoAsync(boleto.AccountNumber);

                Console.WriteLine(
                    $"DEBUG: Processing boleto {boleto.BankSlipNumber} for account {boleto.AccountNumber}, user: {userName}");

                response.Data.Add(new BoletoListItemDto
                {
                    PaymentId = boleto.PaymentId,
                    BankSlipNumber = BankSlipNumberManager.ConvertToString(boleto.BankSlipNumber!.Value),
                    Amount = boleto.Amount,
                    CreatedAt = boleto.CreatedAt,
                    DueDate = boleto.DueDate,
                    IsPaid = boleto.IsPaid,
                    PaymentDate = boleto.PaymentDate,
                    Description = boleto.Description,
                    Customer = userName
                });
            }

            response.Message = $"{response.Data.Count} boleto(s) pendente(s) encontrado(s).";
            Console.WriteLine($"DEBUG: Successfully processed {response.Data.Count} pending boletos");
        }
        catch (Exception ex)
        {
            response.Message = "Erro ao buscar boletos pendentes.";
            Console.WriteLine($"LOG ERROR: Falha em GetPendingBoletosAsync: {ex}");
        }

        return response;
    }

    public async Task<ResponseModel<List<BoletoListItemDto>>> GetPaidBankSlipsAsync()
    {
        var response = new ResponseModel<List<BoletoListItemDto>>();
        try
        {
            var userId = getUserData.GetCurrentUserId();
            var paidBoletos = await paymentRepository.GetPaidBankSlipsAsync(userId);
            Console.WriteLine($"DEBUG: Found {paidBoletos.Count} paid boletos for user {userId}");

            response.Data = [];

            foreach (var boleto in paidBoletos)
            {
                var userName = await getUserData.GetUserNameForBoletoAsync(boleto.AccountNumber);
                Console.WriteLine(
                    $"DEBUG: Processing paid boleto {boleto.BankSlipNumber} for account {boleto.AccountNumber}, user: {userName}");

                response.Data.Add(new BoletoListItemDto
                {
                    PaymentId = boleto.PaymentId,
                    BankSlipNumber = BankSlipNumberManager.ConvertToString(boleto.BankSlipNumber!.Value),
                    Amount = boleto.Amount,
                    CreatedAt = boleto.CreatedAt,
                    DueDate = boleto.DueDate,
                    IsPaid = boleto.IsPaid,
                    PaymentDate = boleto.PaymentDate,
                    Description = boleto.Description,
                    AccountNumber = boleto.AccountNumber,
                    Customer = userName
                });
            }

            response.Message = $"{response.Data.Count} boleto(s) pago(s) encontrado(s).";
            Console.WriteLine($"DEBUG: Successfully processed {response.Data.Count} paid boletos");
        }
        catch (Exception ex)
        {
            response.Message = "Erro ao buscar boletos pagos.";
            Console.WriteLine($"LOG ERROR: Falha em GetPaidBoletosAsync: {ex}");
        }

        return response;
    }

    public async Task<ResponseModel<bool>> PayBankSlipAsync(PayBankSlipDto dto)
    {
        var response = new ResponseModel<bool>();

        Console.WriteLine($"DEBUG PAY V1: Starting legacy payment for BankSlipNumber = {dto.BankSlipNumber}");

        // ✅ CONVERTER PARA PayPartialBankSlipDto
        var partialDto = new PayPartialBankSlipDto
        {
            BankSlipNumber = dto.BankSlipNumber,
            UserPassword = dto.UserPassword,
            AmountToPay = null // null = pagamento integral
        };

        var coreResult = await PayBankSlipCoreAsync(partialDto);

        response.Data = coreResult.IsSuccess;
        response.Message = coreResult.Message;

        Console.WriteLine($"DEBUG PAY V1: Legacy payment completed - Success: {coreResult.IsSuccess}");

        return response;
    }

    public async Task<ResponseModel<PayBankSlipResponseDto>> PayPartialBankSlipAsync(PayPartialBankSlipDto dto)
    {
        var response = new ResponseModel<PayBankSlipResponseDto>();
        var coreResult = await PayBankSlipCoreAsync(dto);

        response.Data = coreResult.IsSuccess ? coreResult : null;

        // ✅ MENSAGEM SIMPLES E GENÉRICA:
        response.Message = coreResult.IsSuccess ? "Operação realizada com sucesso." : "Falha na operação.";

        return response;
    }

    private async Task<PayBankSlipResponseDto> PayBankSlipCoreAsync(PayPartialBankSlipDto dto)
    {
        var result = new PayBankSlipResponseDto();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            Console.WriteLine($"DEBUG PAY CORE: Starting payment for BankSlipNumber = {dto.BankSlipNumber}");
            Console.WriteLine(
                $"DEBUG PAY CORE: Requested amount to pay = {dto.AmountToPay?.ToString("C") ?? "FULL AMOUNT"}");

            // 1. ✅ CONVERTER E VALIDAR BANKSLIPNUMBER
            var bankSlipNumberLong = BankSlipNumberManager.ConvertToLong(dto.BankSlipNumber);
            if (bankSlipNumberLong is null)
            {
                result.IsSuccess = false;
                result.Message = "Número de boleto inválido.";
                return result;
            }

            // 2. ✅ BUSCAR BOLETO NO PAYMENTSBANK
            var existingPayment = await paymentRepository.GetBankSlipByNumberAsync(bankSlipNumberLong.Value);
            if (existingPayment is null)
            {
                result.IsSuccess = false;
                result.Message = "Nenhum boleto pendente foi encontrado.";
                return result;
            }

            Console.WriteLine(
                $"DEBUG PAY CORE: Boleto FOUND - Amount: {existingPayment.Amount}, IsPaid: {existingPayment.IsPaid}");

            if (existingPayment.IsPaid)
            {
                result.IsSuccess = false;
                result.Message = "Este boleto já foi pago anteriormente.";
                return result;
            }

            // 3. ✅ DETERMINAR VALOR A PAGAR (DIRETO DO DTO)
            var originalAmount = existingPayment.Amount;
            var valueToPayment = dto.AmountToPay ?? originalAmount; // ✅ USAR DTO DIRETAMENTE

            Console.WriteLine(
                $"DEBUG PAY CORE: Original amount: {originalAmount:C}, Amount to pay: {valueToPayment:C}");

            // 4. ✅ VALIDAÇÕES DE PAGAMENTO PARCIAL
            if (valueToPayment <= 0)
            {
                result.IsSuccess = false;
                result.Message = "O valor a pagar deve ser maior que zero.";
                return result;
            }

            if (valueToPayment > originalAmount)
            {
                result.IsSuccess = false;
                result.Message =
                    $"O valor a pagar (R$ {valueToPayment:F2}) não pode ser maior que o valor do boleto (R$ {originalAmount:F2}).";
                return result;
            }

            // 5. ✅ CHAMAR BANK.API COM NOSSA PROCEDURE
            var paymentDto = new PaymentDto
            {
                AccountNumber = existingPayment.AccountNumber,
                Value = valueToPayment,
                UserPassword = dto.UserPassword // ✅ USAR DTO DIRETAMENTE
            };

            Console.WriteLine(
                $"DEBUG PAY CORE: Calling Bank.Api procedure - Account: {paymentDto.AccountNumber}, Value: {paymentDto.Value:C}");

            var bankResult = await bankApiClient.ProcessPaymentAsync(paymentDto);

            if (bankResult.Data?.IsSuccess != true)
            {
                Console.WriteLine($"DEBUG PAY CORE: Bank procedure failed: {bankResult.Message}");
                result.IsSuccess = false;
                result.Message = bankResult.Data?.ErrorMessage ?? bankResult.Message;
                return result;
            }

            Console.WriteLine($"DEBUG PAY CORE: Bank procedure successful!");

            // 6. ✅ ATUALIZAR BOLETO NO PAYMENTSBANK
            var newAmount = originalAmount - valueToPayment;
            var isFullyPaid = newAmount <= 0;

            Console.WriteLine(
                $"DEBUG PAY CORE: Updating boleto - New amount: {newAmount:C}, Fully paid: {isFullyPaid}");

            existingPayment.Amount = newAmount;
            existingPayment.IsPaid = isFullyPaid;
            existingPayment.PaymentDate = isFullyPaid ? DateTime.UtcNow : existingPayment.PaymentDate;
            existingPayment.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 7. ✅ BUSCAR DADOS DO USUÁRIO
            var userName = await getUserData.GetUserNameForBoletoAsync(existingPayment.AccountNumber);

            // 8. ✅ MONTAR RESULTADO COMPLETO
            result.IsSuccess = true;
            result.Message = isFullyPaid
                ? "Boleto pago integralmente com sucesso!"
                : $"Pagamento parcial realizado. Restam R$ {newAmount:F2} para quitar o boleto.";
            result.BankSlipNumber = dto.BankSlipNumber; // ✅ USAR DTO DIRETAMENTE
            result.AccountNumber = existingPayment.AccountNumber;
            result.Customer = userName;
            result.OriginalAmount = originalAmount;
            result.AmountPaid = valueToPayment;
            result.RemainingAmount = newAmount;
            result.IsFullyPaid = isFullyPaid;
            result.PaymentDate = DateTime.UtcNow;
            result.Description = existingPayment.Description ?? "";

            Console.WriteLine(
                $"DEBUG PAY CORE: Payment completed - User: {userName}, Amount paid: {valueToPayment:C}, Fully paid: {isFullyPaid}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.IsSuccess = false;
            result.Message = "Ocorreu um erro ao processar o pagamento do boleto.";
            Console.WriteLine($"DEBUG PAY CORE ERROR: {ex.Message}");
        }

        return result;
    }
}