using Bank.Api.DTOS;
using Bank.Api.Services.Movimentations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MovimentationController : ControllerBase
    {
        public readonly IMovimentationService _movimentationService;

        public MovimentationController(IMovimentationService movimentationService)
        {
            _movimentationService = movimentationService;
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] MovimentationDto data)
        {
            var response = await _movimentationService.MovimentationDepositAsync(data);
            return Ok(response);
        }

        [HttpPost("whitdraw")]
        public async Task<IActionResult> Whitdrawn([FromBody] MovimentationDto data)
        {
            var response = await _movimentationService.MovimentationWithdrawAsync(data);
            return Ok(response);
        }
    }
}
