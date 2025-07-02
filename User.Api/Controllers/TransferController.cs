using Bank.Api.DTOS;
using Bank.Api.Services.TransferServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpPost("Transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDTO transferRequest)
        {
            var response = await _transferService.PerformTransferAsync(transferRequest);
            return Ok(response);
        }
    }
}
