using Bank.Api.DTOS;
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

        [HttpGet("GetAccountByCpf/{cpf}")]
        public async Task<IActionResult> GetAccountByCpf(string cpf)
        {
            var response = await _userAccountService.GetAccountByCpfAsync(cpf);
            return Ok(response);
        }

        [HttpGet("GetAccountByEmail/{email}")]
        public async Task<IActionResult> GetAccountByEmail(string email)
        {
            var response = await _userAccountService.GetAccountByEmailAsync(email);
            return Ok(response);
        }


        [HttpPut("update-token/{accountNumber}")]
        public async Task<IActionResult> UpdateToken(string accountNumber, [FromBody] UpdateTokenDto dto)
        {
            await _userAccountService.UpdateTokenAsync(accountNumber, dto.Token);
            return NoContent();
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUserAccount(CreateAccountUserDto userAccountDto)
        {
            var response = await _userAccountService.CreateUserWithAccountAsync(userAccountDto);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetAccountByNumber")]
        public async Task<IActionResult> GetAccountByNumber()
        {
            var account = await _userAccountService.GetUserByAccountAsync();

            return Ok(account);
        }


        [Authorize]
        [HttpDelete("delete-user/{accountNumber}")]
        public async Task<IActionResult> DeleteUserAsync(string accountNumber)
        {
            var response = await _userAccountService.DeleteUserAsync(accountNumber);

            if (!response.Data)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }
        
        [Authorize]
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserDto updateUserDto)
        {
            var response = await _userAccountService.UpdateUserAsync(updateUserDto);
            
            if (!response.Data)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
        
        [Authorize] 
        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePasswordAsync([FromBody] UpdatePasswordDto updatePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userAccountService.UpdatePasswordAsync(updatePasswordDto);

            if (!response.Data)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserByIdAsync()
        {
            var response = await _userAccountService.GetUserByIdAsync();
            return Ok(response);
        }

    }
}
