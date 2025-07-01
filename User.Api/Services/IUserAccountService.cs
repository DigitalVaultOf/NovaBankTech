using User.Api.DTOS;

namespace User.Api.Services
{
    public interface IUserAccountService
    {
        Task CreateUserWithAccountAsync(CreateAccountUserDto dto);
        Task<AccountResponseDto> GetUserByAccountAsync(string accountNumber);
        Task<AccountLoginDto> GetAccountByLoginAsync(string accountNumber);
        Task UpdateTokenAsync(string accountNumber, string token);
    }
}
