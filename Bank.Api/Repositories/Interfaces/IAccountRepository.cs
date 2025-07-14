using User.Api.Model;

namespace User.Api.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task CreateAccount(Account account);
        Task<string> GenerateAccountNumberAsync();
        Task<Account?> GetByAccountNumberWithUserAsync(string accountNumber);
        Task<Account?> GetByAccountLoginInfo(string accountNumber);
        Task<List<string>> GetAccountNumbersByEmailLoginInfo(string email);
        Task<List<string>> GetAccountNumbersByCpfLoginInfo(string cpf);
        Task UpdateTokenAsync(string accountNumber, string token);
        Task UpdateAccountAsync(Account account);
    }
}
