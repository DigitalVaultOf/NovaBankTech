using Bank.Api.DTOS;
using Microsoft.EntityFrameworkCore;
using User.Api.Data;
using User.Api.Model;

namespace Bank.Api.Services.Movimentations
{
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
            {
                var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;

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
        }

        public async Task<ResponseModel<string>> MovimentationWithdrawAsync(MovimentationDto data)
        {
            var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;
            var response = new ResponseModel<string>();
            try
            {
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
    }
}