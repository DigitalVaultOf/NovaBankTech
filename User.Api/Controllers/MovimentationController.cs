using Bank.Api.DTOS;
using Bank.Api.Services.HistoryMovementationService;
using Bank.Api.Services.Movimentations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Bank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MovimentationController : ControllerBase
    {
        public readonly IMovimentationService _movimentationService;
        private readonly IMovimention _movimention;
        public MovimentationController(IMovimentationService movimentationService, IMovimention movimention)
        {
            _movimentationService = movimentationService;
            _movimention = movimention;
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

        [HttpGet ("ListMovementaion")]
        public async Task<IActionResult> GetMovimentationsAsync()
        {
            var response= await _movimention.GetMovimentationsAsync();
            return Ok(response);
        }

        [HttpGet("ListMovementaion/1weak")]
        public async Task<IActionResult> GetMovimentations1weakAsync()
        {
            var response = await _movimention.GetMovimentations1WeakAsync();
            return Ok(response);
        }

        [HttpGet("ListMovementaion/1mounth")]
        public async Task<IActionResult> GetMovimentations1mounthAsync()
        {
            var response = await _movimention.GetMovimentations1MounthAsync();
            return Ok(response);
        }
    }
}
