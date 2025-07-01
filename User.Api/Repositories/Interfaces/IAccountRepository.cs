using User.Api.Model;

namespace User.Api.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task CreateAccount(Account account);
        Task<string> GenerateAccountNumberAsync();
        Task<Account> GetByAccountNumberWithUserAsync(string accountNumber);
        Task<Account> GetByAccountLoginInfo(string accountNumber);
        Task UpdateTokenAsync(string accountNumber, string token);

    }
}
