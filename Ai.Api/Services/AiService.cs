using Ai.Api.DTOS;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Ai.Api.Services;

public class AiService(IConfiguration configuration, HttpClient httpClient, IMemoryCache cache) : IAiService
{
    public async Task<ChatbotResponseDto> AskQuestionAsync(AskQuestionDto questionDto)
    {
        var cacheKey = $"gemini_response_{questionDto.Question.GetHashCode()}";
        if (cache.TryGetValue(cacheKey, out ChatbotResponseDto? cachedResponse) && cachedResponse is not null)
        {
            return cachedResponse;
        }

        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("Chave da API do Gemini não configurada.");
        }

        var prompt = $"""
                      Você é a assistente virtual oficial da NovaBankTech, nosso banco digital.

                      REGRAS IMPORTANTES:
                      - Responda APENAS sobre as funcionalidades listadas abaixo
                      - Se a pergunta for sobre outros assuntos (clima, política, receitas, etc.), responda: "Sou especializada apenas em assuntos da NovaBankTech. Como posso ajudá-la com nossos serviços bancários?"
                      - Se perguntarem sobre outros bancos, responda: "Posso ajudar apenas com serviços da NovaBankTech. Que funcionalidade gostaria de conhecer?"
                      - Mantenha respostas objetivas e profissionais
                      - Use sempre "nossa plataforma", "A NovaBankTech" ou "nossos serviços" ao se referir à empresa

                      FUNCIONALIDADES DA NOVABANKTECH:
                      1. 💳 Pagamento de Boletos: Digite o número do boleto e sua senha para pagar
                      2. 📋 Histórico de Pagamentos: Consulte boletos pagos e pendentes
                      3. 💸 Transferências: Envie dinheiro para outras contas da NovaBankTech
                      4. 💰 Saques e Depósitos: Disponíveis na tela inicial
                      5. ⚡ PIX: Transferências instantâneas 24h
                      6. ⚙️ Gestão de Conta: Edite dados pessoais e altere senha em Configurações
                      7. ❌ Desativar Conta: Ação irreversível disponível nas configurações
                      8. 📊 Consulta de Saldo: Sempre visível no topo da página inicial
                      9. 📄 Exportar Histórico: Baixe suas movimentações bancárias no formato .pdf ou .xlsx (Excel)

                      PERGUNTA DO CLIENTE: "{questionDto.Question}"

                      Responda em até 80 palavras, sendo útil e direta.
                      """;

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

        const int maxRetries = 3;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = ParseGeminiResponse(responseContent);
                    cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                    return result;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    if (attempt >= maxRetries)
                        return new ChatbotResponseDto(
                            "Desculpe, estou temporariamente sobrecarregado. " +
                            "Por favor, tente novamente em alguns minutos. " +
                            "Posso ajudá-lo com questões sobre pagamentos, transferências, PIX e outras funcionalidades do NovaBankTech."
                        );
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt) * 5); // 5s, 10s, 20s
                    await Task.Delay(delay);
                    continue;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API do Gemini: {response.StatusCode} - {errorContent}");
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
            }
            catch (HttpRequestException)
            {
                return new ChatbotResponseDto(
                    "Desculpe, estou com problemas de conectividade. " +
                    "Tente novamente em alguns minutos ou entre em contato com o suporte."
                );
            }
        }

        return new ChatbotResponseDto(
            "Olá! Sou o assistente do NovaBankTech. " +
            "Posso ajudá-lo com pagamentos de boletos, transferências, PIX, consulta de saldo e outras funcionalidades do banco. " +
            "Como posso ajudá-lo hoje?"
        );
    }

    private static ChatbotResponseDto ParseGeminiResponse(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() <= 0)
                return new ChatbotResponseDto("Não foi possível processar sua pergunta. Tente reformulá-la.");
            var firstCandidate = candidates[0];
            if (!firstCandidate.TryGetProperty("content", out var contentProp) ||
                !contentProp.TryGetProperty("parts", out var parts) ||
                parts.GetArrayLength() <= 0)
                return new ChatbotResponseDto("Não foi possível processar sua pergunta. Tente reformulá-la.");
            var firstPart = parts[0];
            return firstPart.TryGetProperty("text", out var text)
                ? new ChatbotResponseDto(text.GetString() ?? "Resposta vazia")
                : new ChatbotResponseDto("Não foi possível processar sua pergunta. Tente reformulá-la.");
        }
        catch (JsonException)
        {
            return new ChatbotResponseDto("Erro ao processar resposta. Tente novamente.");
        }
    }
}