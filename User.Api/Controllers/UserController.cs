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

        [HttpPut("update-token/{accountNumber}")]
        public async Task<IActionResult> UpdateToken(string accountNumber, [FromBody] UpdateTokenDto dto)
        {
            await _userAccountService.UpdateTokenAsync(accountNumber, dto.Token);
            return NoContent();
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUserAccount(CreateAccountUserDto userAccountDto)
        {
            await _userAccountService.CreateUserWithAccountAsync(userAccountDto);
            return Ok("Usuário e Conta foram criados com sucesso!");
        }

        [Authorize]
        [HttpGet("GetAccountByNumber/{accountNumber}")]
        public async Task<IActionResult> GetAccountByNumber(string accountNumber)
        {
            var account = await _userAccountService.GetUserByAccountAsync(accountNumber);

            return Ok(account);
        }


        //[Authorize]
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
        
        //[Authorize]
        [HttpPut("update-user/{id:guid}")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            var response = await _userAccountService.UpdateUserAsync(id, updateUserDto);
            
            if (!response.Data)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
        
        // [Authorize] REMOVER COMENTARIO DEPOIS.
        [HttpPost("update-password/{id:guid}")]
        public async Task<IActionResult> UpdatePasswordAsync(Guid id, [FromBody] UpdatePasswordDto updatePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userAccountService.UpdatePasswordAsync(id, updatePasswordDto);

            if (!response.Data)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
        

    }
}
