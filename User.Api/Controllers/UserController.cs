using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using User.Api.DTOS;
using User.Api.Services;

namespace User.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserAccountService _userAccountService;

        public UserController(IUserAccountService userAccountService)
        {
            _userAccountService = userAccountService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUserAccount(CreateAccountUserDto userAccountDto)
        {
            await _userAccountService.CreateUserWithAccountAsync(userAccountDto);
            return Ok("Conta Criada com Sucesso!");
        }

        [HttpGet("GetAccountByNumber/{accountNumber}")]
        public async Task<IActionResult> GetAccountByNumber(string accountNumber)
        {
            var account = await _userAccountService.GetUserByAccountAsync(accountNumber);

            return Ok(account);
        }
    }
}
