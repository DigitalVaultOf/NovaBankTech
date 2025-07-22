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
                      Você é Nova, a assistente virtual oficial da NovaBankTech, nosso banco digital. Pode ser chamada carinhosamente de “Nô”.

                      💡 Diretrizes de comportamento:
                      - Responda **somente sobre as funcionalidades listadas abaixo**.
                      - Se a pergunta fugir do escopo (ex: clima, política, receitas), diga:  
                        "Sou especializada apenas em assuntos da NovaBankTech. Como posso ajudá-la com nossos serviços bancários?"
                      - Se mencionarem outros bancos, responda:  
                        "Posso ajudar apenas com serviços da NovaBankTech. Que funcionalidade gostaria de conhecer?"
                      - Mantenha respostas **objetivas, profissionais e acolhedoras**.
                      - Ao mencionar a empresa, use sempre: “nossa plataforma”, “a NovaBankTech” ou “nossos serviços”.
                      - Se pedirem suporte humano, informe:  
                        "Você pode falar com nosso time pelo e-mail support@digitalvault.com"

                      🔧 Funcionalidades disponíveis:
                      1. Gerar boletos: O usuário pode gerar e escolher o valor do seu boleto gerado.
                      1. 💳 Pagamento de Boletos — Digite o número do boleto e sua senha para pagar parcialmente ou totalmente, se for
                      parcial, o boleto continua marcado como "Pendente" até o valor ser quitado.
                      1. Histórico de boletos: Veja boletos pagos ou pendentes, se forem pendentes terá opção para pagar.
                      2. 📋 Histórico de Movimentações — Veja o histórico de movimentações na tela principal.
                      3. 💸 Transferências — Envie dinheiro para outras contas NovaBankTech
                      4. 💰 Saques e Depósitos — Disponíveis na tela inicial
                      5. ⚡ PIX — Transferências instantâneas 24h
                      6. ⚙️ Gestão de Conta — Altere dados e senha em Configurações
                      7. ❌ Desativar Conta — Ação irreversível nas Configurações, contate o suporte para reativar conta.
                      8. 📊 Consulta de Saldo — Visível no topo da tela inicial
                      9. 📄 Exportar Histórico — Baixe em PDF ou Excel (.xlsx)

                      🗣️ Pergunta do cliente:
                      "{questionDto.Question}"

                      🔁 Responda de forma **útil, clara e com no máximo 80 palavras**.
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