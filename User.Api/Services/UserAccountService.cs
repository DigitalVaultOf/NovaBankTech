using Microsoft.EntityFrameworkCore;
using User.Api.CamposEnum;
using User.Api.Data;
using User.Api.DTOS;
using User.Api.Model;
using User.Api.Repositories.Interfaces;

namespace User.Api.Services
{
    public class UserAccountService : IUserAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;

        public UserAccountService(AppDbContext context, IUserRepository userRepository, IAccountRepository accountRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _accountRepository = accountRepository;
        }

        public async Task CreateUserWithAccountAsync(CreateAccountUserDto dto)
        {
            var cpfExiste = await _context.Users.AnyAsync(u => u.Cpf == dto.Cpf);
            if (cpfExiste)
            {
                throw new Exception("CPF já cadastrado.");
            }

            var emailExiste = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExiste)
            {
                throw new Exception("Email já cadastrado.");
            }

            var user = new Users
            {
                Name = dto.Name,
                Cpf = dto.Cpf,
                Email = dto.Email,
            };


            var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var accountNumber = await _accountRepository.GenerateAccountNumberAsync();

            //Conta Corrente
            var checkingAccount = new Account
            {
                AccountNumber = accountNumber,
                AccountType = AccountTypeEnum.CheckingAccount,
                Balance = dto.InitialBalance,
                SenhaHash = senhaHash,
            };

            //Conta Poupança
            var savingsAccount = new Account
            {
                AccountNumber = accountNumber,
                AccountType = AccountTypeEnum.SavingsAccount,
                Balance = dto.InitialBalance,
                SenhaHash = senhaHash,
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _userRepository.CreateUser(user);
                checkingAccount.UserId = user.Id;
                savingsAccount.UserId = user.Id;
                await _accountRepository.CreateAccount(checkingAccount);
                await _accountRepository.CreateAccount(savingsAccount);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Erro ao criar usuário e contas: {ex.Message}");
            }

        }
    }
}
