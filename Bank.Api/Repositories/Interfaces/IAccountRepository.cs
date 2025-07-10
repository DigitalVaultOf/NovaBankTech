using User.Api.Model;

namespace User.Api.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task CreateAccount(Account account);
        Task<string> GenerateAccountNumberAsync();
        Task<Account?> GetByAccountNumberWithUserAsync(string accountNumber);
        Task<Account?> GetByAccountLoginInfo(string accountNumber);
        Task<Account?> GetByEmailLoginInfo(string email);
        Task<Account?> GetByCpfLoginInfo(string cpf);
        Task UpdateTokenAsync(string accountNumber, string token);
        Task UpdateAccountAsync(Account account);
    }
}
