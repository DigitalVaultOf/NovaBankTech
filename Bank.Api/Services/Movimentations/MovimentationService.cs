using System.Data;
using Bank.Api.DTOS;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using User.Api.Data;
using User.Api.Model;

namespace Bank.Api.Services.Movimentations;

public class MovimentationService : IMovimentationService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MovimentationService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ResponseModel<string>> MovimentationDepositAsync(MovimentationDto data)
    {
        var response = new ResponseModel<string>();
        try
        {
            var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")
                ?.Value;

            var remetente = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

            bool senhaCorreta = BCrypt.Net.BCrypt.Verify(data.Password, remetente.SenhaHash);
            if (!senhaCorreta)
            {
                response.Message = "Senha inválida.";
                return response;
            }

            var sql = "EXEC MovimentationsDepositProcedure @p0, @p1, @p2";
            await _context.Database.ExecuteSqlRawAsync(sql,
                data.acountNumber = accountNumber,
                data.value,
                data.type = 0);


            response.Data = "Transferência realizada com sucesso.";
            return response;
        }
        catch (Exception e)
        {
            response.Message = $"Erro ao realizar a transferência: {e.Message}";
            return response;
        }
    }


    public async Task<ResponseModel<string>> MovimentationWithdrawAsync(MovimentationDto data)
    {
        var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;
        var response = new ResponseModel<string>();
        try
        {
            var remetente = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

            bool senhaCorreta = BCrypt.Net.BCrypt.Verify(data.Password, remetente.SenhaHash);
            if (!senhaCorreta)
            {
                response.Message = "Senha inválida.";
                return response;
            }

            var sql = "EXEC MovimentationsWithdrawProcedure @p0, @p1, @p2";
            await _context.Database.ExecuteSqlRawAsync(sql,
                data.acountNumber = accountNumber,
                data.value,
                data.type = (CamposEnum.MovimentTypeEnum)1);

            response.Data = "Transferência realizada com sucesso.";
            return response;
        }
        catch (Exception e)
        {
            response.Message = $"Erro ao realizar a transferência: {e.Message}";
            return response;
        }
    }

    public async Task<ResponseModel<PaymentResultDto>> ProcessPaymentAsync(PaymentDto data)
    {
        var response = new ResponseModel<PaymentResultDto>();

        try
        {
            // 1. Validações básicas
            if (string.IsNullOrEmpty(data.AccountNumber))
            {
                response.Message = "O número da conta é obrigatório.";
                response.Data = new PaymentResultDto { IsSuccess = false, ErrorMessage = response.Message };
                return response;
            }

            // 2. ✅ VALIDAR SENHA NO C# (como sempre fizemos)
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == data.AccountNumber);

            if (account is null)
            {
                response.Message = "Conta não encontrada.";
                response.Data = new PaymentResultDto { IsSuccess = false, ErrorMessage = response.Message };
                return response;
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(data.UserPassword, account.SenhaHash);

            if (!isPasswordValid)
            {
                response.Message = "Senha inválida.";
                response.Data = new PaymentResultDto { IsSuccess = false, ErrorMessage = response.Message };
                return response;
            }

            Console.WriteLine($"DEBUG BANK: Processing payment - Account: {data.AccountNumber}, Value: {data.Value:C}");

            // 3. ✅ EXECUTAR PROCEDURE SIMPLES (SÓ 2 PARÂMETROS)
            var accountNumberParam = new SqlParameter("@AccountNumber", data.AccountNumber);
            var valueParam = new SqlParameter("@Value", data.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC ProcessPaymentProcedure @AccountNumber, @Value",
                accountNumberParam,
                valueParam
            );

            // 4. ✅ SE CHEGOU AQUI = SUCESSO (não lançou exception)
            Console.WriteLine($"DEBUG BANK: Procedure executed successfully!");

            response.Data = new PaymentResultDto
            {
                IsSuccess = true,
                ErrorMessage = ""
            };

            response.Message = "Pagamento processado com sucesso.";

            Console.WriteLine($"DEBUG BANK: Payment completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LOG ERROR: Falha em ProcessPaymentAsync: {ex}");

            // ✅ CAPTURAR MENSAGEM ESPECÍFICA DO THROW
            var errorMessage = ex.InnerException?.Message ?? ex.Message;

            Console.WriteLine($"DEBUG BANK: Procedure failed with error: {errorMessage}");

            response.Message = $"Erro ao processar pagamento: {errorMessage}";
            response.Data = new PaymentResultDto
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }

        return response;
    }

    public async Task<ResponseModel<bool>> ProcessBankSlipPaymentAsync(PaymentDto data)
    {
        var result = await ProcessPaymentAsync(data);

        return new ResponseModel<bool>
        {
            Data = result.Data?.IsSuccess ?? false,
            Message = result.Message
        };
    }
}