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

        public async Task<ResponseModel<string>> CreateUserWithAccountAsync(CreateAccountUserDto dto)
        {
            ResponseModel<string> response = new ResponseModel<string>();

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
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Cpf = dto.Cpf,
                Email = dto.Email,
            };


            var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var accountNumberCheckingAccount = await _accountRepository.GenerateAccountNumberAsync();
            var accountNumberSavingsAccount = await _accountRepository.GenerateAccountNumberAsync();

            //Conta Corrente
            var checkingAccount = new Account
            {
                AccountNumber = accountNumberCheckingAccount,
                AccountType = AccountTypeEnum.CheckingAccount,
                SenhaHash = senhaHash,
            };

            //Conta Poupança
            var savingsAccount = new Account
            {
                AccountNumber = accountNumberSavingsAccount,
                AccountType = AccountTypeEnum.SavingsAccount,
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

                response.Message = "Usuário e contas criados com sucesso!";
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Message = ($"Erro ao criar usuário e contas: {ex.Message} | Inner: {ex.InnerException?.Message}");
                return response;
            }

        }

        public async Task<ResponseModel<AccountResponseDto>> GetUserByAccountAsync(string accountNumber)
        {
            ResponseModel<AccountResponseDto> response = new ResponseModel<AccountResponseDto>();

            try
            {

                var account = await _accountRepository.GetByAccountNumberWithUserAsync(accountNumber);
                if (account == null)
                {
                    throw new Exception("Conta não encontrada.");
                }

                var responseDto = new AccountResponseDto
                {
                    AccountNumber = account.AccountNumber,
                    AccountType = account.AccountType.ToString(),
                    Name = account.User.Name,
                    Balance = account.Balance
                };

                response.Data = responseDto;
                response.Message = "Usuário encontrado com sucesso!";

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Erro ao buscar usuário pela conta: {ex.Message} | Inner: {ex.InnerException?.Message}";
                return response;
            }
            
        }

        public async Task<ResponseModel<AccountLoginDto>> GetAccountByLoginAsync(string accountNumber)
        {
            ResponseModel<AccountLoginDto> response = new ResponseModel<AccountLoginDto>();

            try
            {
                var account = await _accountRepository.GetByAccountLoginInfo(accountNumber);
                if (account == null)
                {
                    return null;
                }

                var dto = new AccountLoginDto
                {
                    AccountNumber = account.AccountNumber,
                    SenhaHash = account.SenhaHash
                };

                response.Data = dto;
                response.Message = "Conta encontrada com sucesso!";

                return response;
            }
            catch (Exception ex)
            {

                response.Message = $"Erro ao buscar conta por login: {ex.Message} | Inner: {ex.InnerException?.Message}";
                return response;
            }
        }

        public async Task UpdateTokenAsync(string accountNumber, string token)
        {
            await _accountRepository.UpdateTokenAsync(accountNumber, token);
        }
    }
}
