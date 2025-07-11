using Bank.Api.DTOS;
using Bank.Api.Services.PixServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Api.Controllers
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

        [HttpPost("registrar")]
        public async Task<IActionResult> criarPix([FromBody] RegistroPixDto data)
        {
            var response = await _pixService.CriarPix(data);
            return Ok(response);
        }
    }
}
