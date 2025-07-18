using Bank.Api.DTOS;
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
                response.Message = "Senha incorreta.";
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
                response.Message = "Senha incorreta.";
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

    public async Task<ResponseModel<bool>> ProcessDebitPaymentAsync(DebitPaymentDto data)
    {
        var response = new ResponseModel<bool>();
        try
        {
            if (string.IsNullOrEmpty(data.AccountNumber))
            {
                response.Message = "O número da conta é obrigatório para o débito.";
                response.Data = false;
                return response;
            }

            const string sql = "EXEC ProcessPaymentProcedure @p0, @p1";
            await _context.Database.ExecuteSqlRawAsync(sql, data.AccountNumber, data.Value);

            response.Data = true;
            response.Message = "Débito processado com sucesso.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LOG ERROR: Falha em ProcessDebitPaymentAsync: {ex}");
            response.Message = $"Erro ao processar débito: {ex.Message}";
            response.Data = false;
        }

        return response;
    }
}