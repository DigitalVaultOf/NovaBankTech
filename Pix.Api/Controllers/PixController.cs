using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pix.Api.DTOS;
using Pix.Api.Services.PixService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PixController : ControllerBase
    {
        private readonly IPixService _pixService;

        public PixController(IPixService pixService)
        {
            _pixService = pixService;
        }

        [Authorize]
        [HttpPost("registrar")]
        public async Task<IActionResult> CreateKey([FromBody] RegistroPixDto data)
        {
            var response = await _pixService.RegistroPix(data);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("transferir")]
        public async Task<IActionResult> MakeTransfer(TransferDto data)
        {
            var response = await _pixService.RegistroTransferencia(data);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("get")]
        public async Task<IActionResult> GetAccountNumber([FromBody] string number)
        {
            try
            {
                var response = await _pixService.GetNumberAccountAsync(number);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("has")]
        public async Task<IActionResult> HasPixAsync()
        {
            try
            {
                var response = await _pixService.HasPix();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
