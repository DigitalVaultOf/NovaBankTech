using Auth.Api.Dtos;

namespace Auth.Api.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto dto);
    }
}
