using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PixController : ControllerBase
    {
        [Authorize]
        [HttpGet("Teste")]
        public async Task<IActionResult> GetAccountByLogin(string accountNumber)
        {
            var account = accountNumber;
            return Ok(account);
        }
    }
}
