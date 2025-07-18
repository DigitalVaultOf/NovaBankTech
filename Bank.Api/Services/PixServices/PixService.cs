using Bank.Api.DTOS;
using Bank.Api.Services.UserServices;
using Bank.Api.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using User.Api.Data;
using User.Api.Model;
using User.Api.Repositories.Interfaces;

namespace Bank.Api.Services.PixServices
{
    public class PixService : IPixService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly PixClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;

        public PixService(PixClient client, IHttpContextAccessor httpContextAccessor, AppDbContext context, IAccountRepository accountRepository)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _accountRepository = accountRepository;
        }

        public async Task<ResponseModel<string>> CriarPix(RegistroPixDto data)
        {
            var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;
            var account = await _accountRepository.GetByAccountNumberWithUserAsync(accountNumber);
            data.Account = accountNumber;
            data.Bank = "NovaBank";
            data.Name = account.User.Name;

            var data2 = new RegistroPixDto
            {
                Account = data.Account,
                Bank = data.Bank,
                Name = data.Name,
                PixKey = data.PixKey,
            };

            var response = new ResponseModel<string>();
            if (string.IsNullOrWhiteSpace(data.Name) || string.IsNullOrWhiteSpace(data.PixKey) || string.IsNullOrWhiteSpace(data.Bank))
            {
                response.Message = "Campos obrigatórios estão ausentes";
                return response;
            }
            try
            {
                await _client.CriarPixAsync(data2);
                response.Data = "Pix Criado com sucesso";
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(data);
                response.Message = $"Não foi possivel criar pix: {e.Message}";
                return response;
            }
        }

        public async Task<ResponseModel<string>> GetAccount(string chave)
        {
            var response = new ResponseModel<string>();
            try
            {
                var pesquisa = await _client.Pesquisar(chave);
                response.Data = pesquisa.Data;
                return response;
            }
            catch (Exception e)
            {
                response.Message = $"Error: {e.Message}";
                return response;
            }
        }

        public async Task<ResponseModel<string>> MakeTransfer(MakePixDto data)
        {
            var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;
            var response = new ResponseModel<string>();
            try
            {
                var go = await GetAccount(data.Going);
                if (go.Data != null)
                {
                    var userAccount = await _context.Accounts.Where(a => a.AccountNumber == go.Data).FirstOrDefaultAsync();
                    if (userAccount != null)
                    {
                        var sql = "EXEC MovimentationsDepositProcedure @p0, @p1, @p2";
                        await _context.Database.ExecuteSqlRawAsync(sql,
                            go.Data,
                            data.Amount,
                            2);
                    }
                    var sql2 = "EXEC MovimentationsWithdrawProcedure @p0, @p1, @p2";
                    await _context.Database.ExecuteSqlRawAsync(sql2,
                        accountNumber,
                        data.Amount,
                        2);
                    response.Data = "Pix feito com sucesso";
                    _client.MandarPixAsync(data);
                    return response;
                }
                else
                {
                    var sql2 = "EXEC MovimentationsWithdrawProcedure @p0, @p1, @p2";
                    await _context.Database.ExecuteSqlRawAsync(sql2,
                        accountNumber,
                        data.Amount,
                        2);
                    response.Data = "Pix feito com sucesso";
                    _client.MandarPixAsync(data);
                    return response;
                }
            }
            catch (Exception e)
            {
                response.Message = $"Error 1: {e.Message}";
                return response;
            }
        }
    }
}
