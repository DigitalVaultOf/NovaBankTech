using User.Api.DTOS;

namespace User.Api.Services
{
    public interface IUserAccountService
    {
        Task CreateUserWithAccountAsync(CreateAccountUserDto dto);
        Task<AccountResponseDto> GetUserByAccountAsync(string accountNumber);
    }
}
