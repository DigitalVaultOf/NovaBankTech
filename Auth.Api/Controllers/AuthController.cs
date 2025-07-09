using Auth.Api.Dtos;
using Auth.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        
        // MARCOS ESTÁ MEXENDO AQUI
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            var response = await _authService.AuthenticateAsync(loginRequest);
            return !response.IsSuccess ? StatusCode(403, response) : Ok(response);
        }
    }
}
