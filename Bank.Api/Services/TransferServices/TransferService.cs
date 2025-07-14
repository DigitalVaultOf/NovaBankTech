using Bank.Api.DTOS;
using Microsoft.EntityFrameworkCore;
using User.Api.Data;
using User.Api.Model;

namespace Bank.Api.Services.TransferServices
{
    public class TransferService : ITransferService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransferService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseModel<string>> PerformTransferAsync(TransferRequestDTO dto)
        {
            var response = new ResponseModel<string>();
            try
            {

                var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;

                var remetente = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

                bool senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Password, remetente.SenhaHash);
                if (!senhaCorreta)
                {
                    response.Message = "Senha incorreta.";
                    return response;
                }


                var sql = "EXEC RealizarTransferencia @p0, @p1, @p2, @p3";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    dto.AccountNumberFrom = accountNumber,
                    dto.AccountNumberTo,
                    dto.Amount,
                    dto.Description);

                response.Data = "Transferência realizada com sucesso.";
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Erro ao realizar a transferência: {ex.Message}";
                return response;
            }
        }
    }
}
