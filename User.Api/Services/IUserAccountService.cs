using User.Api.DTOS;

namespace User.Api.Services
{
    public interface IUserAccountService
    {
        Task CreateUserWithAccountAsync(CreateAccountUserDto dto);
    }
}
