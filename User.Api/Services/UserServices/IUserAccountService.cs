using Bank.Api.DTOS;
using User.Api.DTOS;
using User.Api.Model;

namespace Bank.Api.Services.UserServices
{
    public interface IUserAccountService
    {
        Task<ResponseModel<bool>> CreateUserWithAccountAsync(CreateAccountUserDto dto);
        Task<ResponseModel<AccountResponseDto>> GetUserByAccountAsync(string accountNumber);
        Task<ResponseModel<AccountLoginDto>> GetAccountByLoginAsync(string accountNumber);
        Task<ResponseModel<bool>> DeleteUserAsync(string accountNumber);
        Task UpdateTokenAsync(string accountNumber, string token);
        Task<ResponseModel<bool>> UpdateUserAsync(Guid userId, UpdateUserDto dto);
        Task<ResponseModel<bool>> UpdatePasswordAsync(Guid userId, UpdatePasswordDto updatePasswordDto);
    }
}
