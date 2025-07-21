using Ai.Api.DTOS;

namespace Ai.Api.Services;

public interface IAiService
{
    Task<ChatbotResponseDto> AskQuestionAsync(AskQuestionDto question);
}