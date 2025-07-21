using Ai.Api.DTOS;
using Ai.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ai.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController(IAiService aiService) : ControllerBase
    {
        [HttpPost("Ask")]
        public async Task<IActionResult> AskQuestionAsync([FromBody] AskQuestionDto question)
        {
            var response = await aiService.AskQuestionAsync(question);
            return Ok(response);
        }
    }
}