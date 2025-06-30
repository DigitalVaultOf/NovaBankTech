using Microsoft.EntityFrameworkCore;
using User.Api.Data;
using User.Api.Model;
using User.Api.Repositories.Interfaces;

namespace User.Api.Repositories.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task CreateAccount(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }
        public async Task<string> GenerateAccountNumberAsync()
        {
            string accountNumber;
            do
            {
                accountNumber = new Random().Next(100000, 999999).ToString();
            } while (await _context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber));

            return accountNumber;
        }
    }
}
