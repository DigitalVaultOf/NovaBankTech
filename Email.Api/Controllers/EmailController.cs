using Email.Api.Model;
using Email.Api.Services.RegisterEmailServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Email.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IRegisterEmailService _registerEmailService;

        public EmailController(IRegisterEmailService registerEmailService)
        {
            _registerEmailService = registerEmailService;
        }

        [HttpPost("send-welcome")]
        public async Task<IActionResult> SendWelcomeEmail([FromBody] EmailRequest dto)
        {
            var sucesso = await _registerEmailService.EnviarEmailBoasVindas(dto.Email, dto.Nome, dto.ContaCorrente, dto.ContaPoupanca);

            if (sucesso)
                return Ok("E-mail enviado com sucesso.");
            else
                return StatusCode(500, "Erro ao enviar e-mail.");
        }
    }
}
