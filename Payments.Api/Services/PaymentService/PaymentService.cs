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
                AmountBeforePay = dto.Amount,
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
                CreatedAt = boleto.CreatedAt,
                DueDate = boleto.DueDate,
                IsFullyPaid = boleto.IsPaid,
                PaymentDate = boleto.PaymentDate,
                Description = boleto.Description,
                AccountNumber = boleto.AccountNumber,
                Customer = await getUserData.GetUserNameForBoletoAsync(boleto.AccountNumber)
            };

            response.Message = boleto.IsPaid
                ? "Este boleto já foi pago anteriormente!"
                : "Este boleto é válido e está pronto para ser pago!";
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
                    AccountNumber = boleto.AccountNumber,
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
                    AmountBeforePay = boleto.AmountBeforePay,
                    AmountAfterPay = boleto.Amount,
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

        // ✅ CRIAR MENSAGEM BASEADA NO RESULTADO:
        if (coreResult.IsSuccess)
        {
            response.Message = coreResult.IsFullyPaid
                ? "Boleto pago integralmente com sucesso!"
                : $"Pagamento parcial realizado. Restam R$ {coreResult.RemainingAmount:F2} para quitar o boleto.";
        }
        else
        {
            // ✅ MAPEAR ERROS BASEADO NO CONTEXTO:
            response.Message = GetErrorMessageFromResult(coreResult);
        }

        Console.WriteLine($"DEBUG PAY V1: Legacy payment completed - Success: {coreResult.IsSuccess}");

        return response;
    }

    private string GetErrorMessageFromResult(PayBankSlipResponseDto result)
    {
        // ✅ USAR ErrorType QUANDO DISPONÍVEL:
        if (!string.IsNullOrEmpty(result.ErrorType))
        {
            return result.ErrorType;
        }

        // ✅ FALLBACK PARA CASOS ANTIGOS:
        if (string.IsNullOrEmpty(result.BankSlipNumber))
            return "Número de boleto inválido.";

        if (string.IsNullOrEmpty(result.AccountNumber))
            return "Nenhum boleto pendente foi encontrado.";

        if (result.IsFullyPaid)
            return "Este boleto já foi pago anteriormente.";

        return "Ocorreu um erro ao processar o pagamento do boleto.";
    }


    public async Task<ResponseModel<PayBankSlipResponseDto>> PayPartialBankSlipAsync(PayPartialBankSlipDto dto)
    {
        var response = new ResponseModel<PayBankSlipResponseDto>();
        var coreResult = await PayBankSlipCoreAsync(dto);

        if (coreResult.IsSuccess)
        {
            response.Data = coreResult;
            response.Message = coreResult.IsFullyPaid
                ? "Boleto pago integralmente com sucesso!"
                : $"Pagamento parcial realizado. Restam R$ {coreResult.RemainingAmount:F2} para quitar o boleto.";
        }
        else
        {
            response.Data = null;
            // ✅ Usar as mensagens específicas que estavam em result.Message:
            response.Message = GetErrorMessage(coreResult); // Função helper
        }

        return response;
    }

// ✅ Função helper para mapear erros:
    private static string GetErrorMessage(PayBankSlipResponseDto result)
    {
        // ✅ PRIORIDADE: Usar ErrorType (mensagem específica da Bank.Api)
        if (!string.IsNullOrEmpty(result.ErrorType))
        {
            return result.ErrorType;
        }

        // ✅ FALLBACK: Mapear baseado no contexto
        if (string.IsNullOrEmpty(result.BankSlipNumber))
            return "Número de boleto inválido.";

        if (string.IsNullOrEmpty(result.AccountNumber))
            return "Nenhum boleto pendente foi encontrado.";

        return result.IsFullyPaid ? "Este boleto já foi pago anteriormente." : "Erro ao processar pagamento.";
    }

    private async Task<PayBankSlipResponseDto> PayBankSlipCoreAsync(PayPartialBankSlipDto dto)
    {
        var result = new PayBankSlipResponseDto();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            Console.WriteLine($"DEBUG PAY CORE: Starting payment for BankSlipNumber = '{dto.BankSlipNumber}'");
            Console.WriteLine($"DEBUG PAY CORE: BankSlipNumber Length = {dto.BankSlipNumber?.Length}");
            Console.WriteLine($"DEBUG PAY CORE: BankSlipNumber Type = {dto.BankSlipNumber?.GetType()}");

            Console.WriteLine($"DEBUG PAY CORE: Raw BankSlipNumber = [{dto.BankSlipNumber}]");
            Console.WriteLine($"DEBUG PAY CORE: Trimmed = [{dto.BankSlipNumber?.Trim()}]");


            // 1. ✅ CONVERTER E VALIDAR BANKSLIPNUMBER
            var bankSlipNumberLong = BankSlipNumberManager.ConvertToLong(dto.BankSlipNumber);
            Console.WriteLine($"DEBUG PAY CORE: ConvertToLong result = {bankSlipNumberLong}");

            if (bankSlipNumberLong is null)
            {
                Console.WriteLine($"DEBUG PAY CORE: VALIDATION FAILED - BankSlipNumber conversion returned null");
                result.IsSuccess = false;
                return result;
            }

            // 2. ✅ BUSCAR BOLETO NO PAYMENTSBANK
            var existingPayment = await paymentRepository.GetBankSlipByNumberAsync(bankSlipNumberLong.Value);
            if (existingPayment is null)
            {
                Console.WriteLine($"DEBUG PAY CORE: BOLETO NOT FOUND");
                result.IsSuccess = false;
                result.BankSlipNumber = dto.BankSlipNumber; // ✅ Tem número, mas não encontrado
                return result;
            }

            Console.WriteLine(
                $"DEBUG PAY CORE: Boleto FOUND - Amount: {existingPayment.Amount}, IsPaid: {existingPayment.IsPaid}");

            if (existingPayment.IsPaid)
            {
                Console.WriteLine($"DEBUG PAY CORE: BOLETO ALREADY PAID");
                result.IsSuccess = false;
                result.IsFullyPaid = true; // ✅ CHAVE para identificar "já pago"
                result.BankSlipNumber = dto.BankSlipNumber;
                result.AccountNumber = existingPayment.AccountNumber;
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
                return result;
            }

            if (valueToPayment > originalAmount)
            {
                result.IsSuccess = false;
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

                // ✅ POPULAR CAMPOS PARA IDENTIFICAR O ERRO:
                result.BankSlipNumber = dto.BankSlipNumber;
                result.AccountNumber = existingPayment.AccountNumber;
                result.OriginalAmount = originalAmount;
                result.AmountPaid = 0;
                result.RemainingAmount = originalAmount;
                result.ErrorType = bankResult.Data?.ErrorMessage ?? bankResult.Message; // ✅ USAR ErrorType

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
            Console.WriteLine($"DEBUG PAY CORE ERROR: {ex.Message}");
        }

        return result;
    }
}