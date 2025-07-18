﻿using System.Diagnostics;
using Bank.Api.DTOS;
using Bank.Api.Services.RabbitMQServices;
using Microsoft.EntityFrameworkCore;
using Shared.Messages.DTOs;
using User.Api.CamposEnum;
using User.Api.Data;
using User.Api.DTOS;
using User.Api.Model;
using User.Api.Repositories.Interfaces;

namespace Bank.Api.Services.UserServices
{
    public class UserAccountService : IUserAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;


        public UserAccountService(AppDbContext context, IUserRepository userRepository,
            IAccountRepository accountRepository, IHttpContextAccessor httpContextAccessor, HttpClient httpClient,
            IConfiguration configuration, IRabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _configuration = configuration;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public async Task<ResponseModel<bool>> CreateUserWithAccountAsync(CreateAccountUserDto dto)
        {   var stopwatch = Stopwatch.StartNew();

            var response = new ResponseModel<bool>();

            var emailApiBaseUrl = _configuration["EmailApi:BaseUrl"];


            var cpfExists = await _context.Users.AnyAsync(u => u.Cpf == dto.Cpf);

            if (cpfExists)
            {
                response.Message = "CPF já cadastrado.";
                response.Data = false;
                return response;
            }

            if (dto == null)
            {
                response.Message = "DTO inválido (nulo).";
                response.Data = false;
                return response;
            }

            if (string.IsNullOrWhiteSpace(dto.Cpf) || string.IsNullOrWhiteSpace(dto.Email))
            {
                response.Message = "CPF ou E-mail não foram fornecidos.";
                response.Data = false;
                return response;
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
            {
                response.Message = "E-mail já cadastrado.";
                response.Data = false;
                return response;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new Users
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Cpf = dto.Cpf,
                    Email = dto.Email,
                };

                await _userRepository.CreateUser(user);

                var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                //Conta Corrente
                var checkingAccount = new Account
                {
                    AccountNumber = await _accountRepository.GenerateAccountNumberAsync(),
                    AccountType = AccountTypeEnum.CheckingAccount,
                    SenhaHash = senhaHash,
                    UserId = user.Id,
                };

                await _accountRepository.CreateAccount(checkingAccount);

                //Conta Poupança
                var savingsAccount = new Account
                {
                    AccountNumber = await _accountRepository.GenerateAccountNumberAsync(),
                    AccountType = AccountTypeEnum.SavingsAccount,
                    SenhaHash = senhaHash,
                    UserId = user.Id,
                };
                await _accountRepository.CreateAccount(savingsAccount);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var emailPayload = new UserCreatedEvent
                {
                    Nome = user.Name,
                    Email = user.Email,
                    ContaCorrente = checkingAccount.AccountNumber,
                    ContaPoupanca = savingsAccount.AccountNumber
                };

                _rabbitMQPublisher.PublishUserCreated(emailPayload); 
               
                response.Data = true;
                response.Message = "Usuário e Conta foram criados com sucesso!";
                return response;
                
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"LOG ERROR: ERRO CRÍTICO em CreateUserWithAccountAsync: {ex}");
                response.Message = $"Erro ao criar usuário e conta. {ex.Message}";
                response.Data = false;
                return response;
            }
        }

        public async Task<ResponseModel<AccountResponseDto>> GetUserByAccountAsync()
        {
            ResponseModel<AccountResponseDto> response = new ResponseModel<AccountResponseDto>();

            try
            {
                var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")
                    ?.Value;

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
                response.Message =
                    $"Erro ao buscar usuário pela conta: {ex.Message} | Inner: {ex.InnerException?.Message}";
                return response;
            }
        }

        public async Task<ResponseModel<AccountLoginDto>> GetAccountByLoginAsync(string accountNumber)
        {
            ResponseModel<AccountLoginDto> response = new ResponseModel<AccountLoginDto>();

            try
            {
                var account = await _accountRepository.GetByAccountNumberWithUserAsync(accountNumber);
                if (account == null)
                {
                    return null;
                }

                var dto = new AccountLoginDto
                {
                    AccountNumber = account.AccountNumber,
                    SenhaHash = account.SenhaHash,
                    UserId = account.UserId,
                    Status = account.User.Status
                };

                response.Data = dto;
                response.Message = "Conta encontrada com sucesso!";

                return response;
            }
            catch (Exception ex)
            {
                response.Message =
                    $"Erro ao buscar conta por login: {ex.Message} | Inner: {ex.InnerException?.Message}";
                return response;
            }
        }

        public async Task<ResponseModel<AccountLoginDto>> GetAccountByCpfAsync(string cpf)
        {
            ResponseModel<AccountLoginDto> response = new ResponseModel<AccountLoginDto>();

            try
            {
                var accountNumbers = await _accountRepository.GetAccountNumbersByCpfLoginInfo(cpf);

                if (accountNumbers == null || !accountNumbers.Any())
                    /* ** */
                {
                    /* ** */
                    /* ** */
                    response.Message = "Nenhuma conta encontrada para o CPF informado."; /* ** */
                    /* ** */
                    return response; /* ** */
                    /* ** */
                } /* ** */

                /* ** */
                var user = await _userRepository.GetUserByCpfAsync(cpf); /* ** */

                /* ** */
                if (user == null) /* ** */
                    /* ** */
                {
                    /* ** */
                    /* ** */
                    response.Message = "Usuário não encontrado para o CPF informado."; /* ** */
                    /* ** */
                    return response; /* ** */
                    /* ** */
                } /* ** */

                /* ** */
                var firstAccountForUser = user.Accounts.FirstOrDefault(); /* ** */


                var dto = new AccountLoginDto
                {
                    /* ** */
                    AccountNumbers = accountNumbers, /* ** */
                    /* ** */
                    Cpf = user.Cpf, /* ** */
                    /* ** */
                    Email = user.Email, /* ** */
                    /* ** */
                    SenhaHash = firstAccountForUser?.SenhaHash, /* ** */
                    /* ** */
                    UserId = user.Id, /* ** */
                    /* ** */
                    Status = user.Status /* ** */
                };

                response.Data = dto;
                /* ** */
                response.Message = "Contas encontradas com sucesso!"; /* ** */

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Erro ao buscar contas por CPF: {ex.Message} | Inner: {ex.InnerException?.Message}";
                return response;
            }
        }

        public async Task<ResponseModel<AccountLoginDto>> GetAccountByEmailAsync(string email)
        {
            ResponseModel<AccountLoginDto> response = new ResponseModel<AccountLoginDto>();

            try
            {
                /* ** */
                var accountNumbers = await _accountRepository.GetAccountNumbersByEmailLoginInfo(email); /* ** */
                /* ** */
                if (accountNumbers == null || !accountNumbers.Any()) /* ** */
                    /* ** */
                {
                    /* ** */
                    /* ** */
                    response.Message = "Nenhuma conta encontrada para o e-mail informado."; /* ** */
                    /* ** */
                    return response; /* ** */
                    /* ** */
                } /* ** */

                /* ** */
                var user = await _userRepository.GetUserByEmailAsync(email); /* ** */

                /* ** */
                if (user == null) /* ** */
                    /* ** */
                {
                    /* ** */
                    /* ** */
                    response.Message = "Usuário não encontrado para o e-mail informado."; /* ** */
                    /* ** */
                    return response; /* ** */
                    /* ** */
                } /* ** */

                /* ** */
                var firstAccountForUser = user.Accounts.FirstOrDefault(); /* ** */

                var dto = new AccountLoginDto
                {
                    AccountNumbers = accountNumbers,
                    Email = user.Email,
                    Cpf = user.Cpf,
                    SenhaHash = firstAccountForUser?.SenhaHash,
                    UserId = user.Id,
                    Status = user.Status
                };

                response.Data = dto;
                response.Message = "Contas encontradas com sucesso!";

                return response;
            }
            catch (Exception ex)
            {
                response.Message =
                    $"Erro ao buscar contas por e-mail: {ex.Message} | Inner: {ex.InnerException?.Message}";
                return response;
            }
        }

        public async Task<ResponseModel<bool>> DeleteUserAsync(string accountNumber)
        {
            var response = new ResponseModel<bool>();

            try
            {
                var account = await _accountRepository.GetByAccountNumberWithUserAsync(accountNumber);


                if (account is null)
                {
                    response.Message = "Conta não encontrada.";
                    response.Data = false;
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(account.UserId);

                if (user is null)
                {
                    response.Message = "Usuário não encontrado.";
                    response.Data = false;
                    return response;
                }

                user.Status = false;

                await _context.SaveChangesAsync();

                response.Message = "Usuário desativado com sucesso!";
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Message = $"Erro ao desativar o usuário: {ex.Message} | Inner: {ex.InnerException?.Message}";
                response.Data = false;
            }

            return response;
        }

        public async Task UpdateTokenAsync(string accountNumber, string token)
        {
            await _accountRepository.UpdateTokenAsync(accountNumber, token);
        }

        public async Task<ResponseModel<bool>> UpdateUserAsync(UpdateUserDto dto)
        {
            var response = new ResponseModel<bool>();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userId = _httpContextAccessor.HttpContext?.User.FindFirst(u => u.Type == "UserId")?.Value;
                var userIdGuid = Guid.Parse(userId);
                var user = await _userRepository.GetUserByIdWithAccountsAsync(userIdGuid);

                if (user is null)
                {
                    response.Message = "Usuário não encontrado.";
                    response.Data = false;
                    return response;
                }

                var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userIdGuid);

                if (emailExists)
                {
                    response.Message = "O e-mail informado já está em uso..";
                    response.Data = false;
                    return response;
                }

                user.Name = dto.Name;
                user.Email = dto.Email;

                await _userRepository.UpdateUserAsync(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Message = "Usuário atualizado com sucesso!";
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"LOG ERROR: ERRO CRÍTICO em UpdateUserAsync para userId: {ex}");
                response.Message = "Ocorreu um erro inesperado ao atualizar o usuário.";
                response.Data = false;
            }

            return response;
        }

        public async Task<ResponseModel<bool>> UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto)
        {
            var response = new ResponseModel<bool>();
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userId = _httpContextAccessor.HttpContext?.User.FindFirst(u => u.Type == "UserId")?.Value;

                var userIdGuid = Guid.Parse(userId);
                var user = await _userRepository.GetUserByIdWithAccountsAsync(userIdGuid);

                if (user is null)
                {
                    response.Data = false;
                    response.Message = "Usuário não encontrado.";
                    return response;
                }

                var account = user.Accounts.FirstOrDefault();

                if (account is null)
                {
                    response.Data = false;
                    response.Message = "Nenhuma conta associada encontrada para este usuário.";
                    return response;
                }

                var isCurrentPasswordValid =
                    BCrypt.Net.BCrypt.Verify(updatePasswordDto.CurrentPassword, account.SenhaHash);

                if (!isCurrentPasswordValid)
                {
                    response.Data = false;
                    response.Message = "Senha atual incorreta.";
                    await transaction.RollbackAsync();
                    return response;
                }

                var isNewPasswordSameAsCurrentPassword =
                    BCrypt.Net.BCrypt.Verify(updatePasswordDto.NewPassword, account.SenhaHash);

                if (isNewPasswordSameAsCurrentPassword)
                {
                    response.Data = false;
                    response.Message = "Sua nova senha deve ser diferente da anterior.";
                    await transaction.RollbackAsync();
                    return response;
                }

                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(updatePasswordDto.NewPassword);
                foreach (var acc in user.Accounts)
                {
                    acc.SenhaHash = newPasswordHash;
                    _context.Accounts.Update(acc);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = true;
                response.Message = "Senha atualizada com sucesso!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"LOG ERROR: ERRO CRÍTICO em UpdatePassword para userId: {ex}");
                response.Data = false;
                response.Message = $"Ocorreu um erro inesperado ao tentar atualizar a senha.";
            }

            return response;
        }

        public async Task<ResponseModel<GetUserDto>> GetUserByIdAsync()
        {
            var response = new ResponseModel<GetUserDto>();

            try
            {
                var userId = _httpContextAccessor.HttpContext?.User.FindFirst(u => u.Type == "UserId")?.Value;
                var userIdGuid = Guid.Parse(userId);
                var user = await _userRepository.GetByIdAsync(userIdGuid);
                var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")
                    ?.Value;

                var dto = new GetUserDto()
                {
                    Id = userIdGuid,
                    AccountNumber = accountNumber,
                    Name = user.Name,
                    Email = user.Email,
                    CPF = user.Cpf,
                };

                response.Data = dto;
                response.Message = "Usuário encontrado com sucesso!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao encontrar dados do usuário: {ex.Message}");
                response.Message = "Ocorreu um erro inesperado.";
            }

            return response;
        }

        public async Task<ResponseModel<List<Account>>> GetAllAcounts()
        {
            var response = new ResponseModel<List<Account>>();
            try
            {
                var accounts = await _context.Accounts.ToListAsync();
                response.Data = accounts;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<List<Users>>> GetAllUsers()
        {
            var response = new ResponseModel<List<Users>>();

            try
            {
                var user = await _context.Users.ToListAsync();
                response.Data = user;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                return response;
                
            }
        }
    }
}