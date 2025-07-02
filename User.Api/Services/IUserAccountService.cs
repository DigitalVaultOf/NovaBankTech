using User.Api.DTOS;
using User.Api.Model;

namespace User.Api.Services
{
    public interface IUserAccountService
    {
        Task<ResponseModel<string>> CreateUserWithAccountAsync(CreateAccountUserDto dto);
        Task<ResponseModel<AccountResponseDto>> GetUserByAccountAsync(string accountNumber);
        Task<ResponseModel<AccountLoginDto>> GetAccountByLoginAsync(string accountNumber);
        Task UpdateTokenAsync(string accountNumber, string token);
    }
}
