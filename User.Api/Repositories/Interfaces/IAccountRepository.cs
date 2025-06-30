using User.Api.Model;

namespace User.Api.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task CreateAccount(Account account);
        Task<string> GenerateAccountNumberAsync();
    }
}
