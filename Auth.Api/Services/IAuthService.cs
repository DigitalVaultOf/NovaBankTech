using Auth.Api.Dtos;
using Auth.Api.Models;

namespace Auth.Api.Services
{
    public interface IAuthService
    {
        Task<ResponseModel<LoginResponseDto>> AuthenticateAsync(LoginRequestDto dto);
    }
}
