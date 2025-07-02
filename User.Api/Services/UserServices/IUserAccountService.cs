using User.Api.DTOS;
using User.Api.Model;

namespace Bank.Api.Services.UserServices
{
    public interface IUserAccountService
    {
        Task<ResponseModel<string>> CreateUserWithAccountAsync(CreateAccountUserDto dto);
        Task<ResponseModel<AccountResponseDto>> GetUserByAccountAsync(string accountNumber);
        Task<ResponseModel<AccountLoginDto>> GetAccountByLoginAsync(string accountNumber);
        Task UpdateTokenAsync(string accountNumber, string token);
    }
}
