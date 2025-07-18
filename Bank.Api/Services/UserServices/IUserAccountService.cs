using Bank.Api.DTOS;
using User.Api.DTOS;
using User.Api.Model;

namespace Bank.Api.Services.UserServices
{
    public interface IUserAccountService
    {
        Task<ResponseModel<bool>> CreateUserWithAccountAsync(CreateAccountUserDto dto);
        Task<ResponseModel<AccountResponseDto>> GetUserByAccountAsync();
        Task<ResponseModel<AccountLoginDto>> GetAccountByLoginAsync(string accountNumber);
        Task<ResponseModel<AccountLoginDto>> GetAccountByCpfAsync(string cpf);
        Task<ResponseModel<AccountLoginDto>> GetAccountByEmailAsync(string email);
        Task<ResponseModel<bool>> DeleteUserAsync(string accountNumber);
        Task UpdateTokenAsync(string accountNumber, string token);
        Task<ResponseModel<bool>> UpdateUserAsync(UpdateUserDto dto);
        Task<ResponseModel<bool>> UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto);
        Task<ResponseModel<GetUserDto>> GetUserByIdAsync();
        Task<ResponseModel<List<Account>>> GetAllAcounts();
        Task<ResponseModel<List<Users>>> GetAllUsers();
    }
}
