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
            // await _context.SaveChangesAsync();
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

        public async Task<Account> GetByAccountLoginInfo(string accountNumber)
        {
            return await _context.Accounts
        .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<Account> GetByAccountNumberWithUserAsync(string accountNumber)
        {
            return await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task UpdateTokenAsync(string accountNumber, string token)
        {
            var account = await GetByAccountNumberWithUserAsync(accountNumber);
            if (account != null)
            {
                account.Token = token;
                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Conta não encontrada.");
            }
        }

        public async Task UpdateAccountAsync(Account account)
        {
            _context.Accounts.Update(account);
            await Task.CompletedTask;
        }

        public async Task<Account?> GetByEmailLoginInfo(string email)
        {
            return await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.User.Email == email);
        }

        public async Task<Account?> GetByCpfLoginInfo(string cpf)
        {
            return await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.User.Cpf == cpf);
        }
    }
}
