using Bank.Api.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using User.Api.DTOS;

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

        [HttpGet("GetAccountByLogin/{accountNumber}")]
        public async Task<IActionResult> GetAccountByLogin(string accountNumber)
        {
            var account = await _userAccountService.GetAccountByLoginAsync(accountNumber);
            return Ok(account);
        }

        [HttpPut("update-token/{accountNumber}")]
        public async Task<IActionResult> UpdateToken(string accountNumber, [FromBody] UpdateTokenDto dto)
        {
            await _userAccountService.UpdateTokenAsync(accountNumber, dto.Token);
            return NoContent();
        }

        
        [HttpPost("create")]
        public async Task<IActionResult> CreateUserAccount(CreateAccountUserDto userAccountDto)
        {
            await _userAccountService.CreateUserWithAccountAsync(userAccountDto);
            return Ok("Conta Criada com Sucesso!");
        }

        [Authorize]
        [HttpGet("GetAccountByNumber/{accountNumber}")]
        public async Task<IActionResult> GetAccountByNumber(string accountNumber)
        {
            var account = await _userAccountService.GetUserByAccountAsync(accountNumber);

            return Ok(account);
        }

        //[Authorize]
        [HttpDelete("delete/{accountNumber}")]
        public async Task<IActionResult> DeleteUserAsync(string accountNumber)
        {
            var response = await _userAccountService.DeleteUserAsync(accountNumber);

            return Ok(response);
        }

    }
}
