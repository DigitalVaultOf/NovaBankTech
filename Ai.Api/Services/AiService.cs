using Ai.Api.DTOS;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Ai.Api.Services;

public class AiService(
    IConfiguration configuration,
    HttpClient httpClient,
    IMemoryCache cache,
    ILogger<AiService> logger) : IAiService
{
    private const int MaxRetries = 3;
    private const int CacheTimeoutMinutes = 10;
    private const string GeminiModel = "gemini-1.5-flash-latest";
    private const string GroqModel = "llama3-8b-8192";

    public async Task<ChatbotResponseDto> AskQuestionAsync(AskQuestionDto questionDto)
    {
        // Verificação mais robusta
        if (string.IsNullOrWhiteSpace(questionDto.Question))
        {
            return CreateFallbackResponse("general_failure");
        }

        // Normaliza a pergunta para melhor cache
        var normalizedQuestion = questionDto.Question.Trim().ToLowerInvariant();
        var cacheKey = $"ai_response_{normalizedQuestion.GetHashCode()}";

        if (cache.TryGetValue(cacheKey, out ChatbotResponseDto? cachedResponse) && cachedResponse is not null)
        {
            logger.LogInformation("Resposta encontrada no cache para: {Question}", questionDto.Question);
            return cachedResponse;
        }

        // Primeiro tenta Gemini
        var geminiResponse = await TryGeminiAsync(questionDto.Question, cacheKey);
        if (geminiResponse != null)
        {
            return geminiResponse;
        }

        logger.LogWarning("Gemini falhou, tentando Groq como fallback para: {Question}", questionDto.Question);

        // Se Gemini falhar, tenta Groq
        var groqResponse = await TryGroqAsync(questionDto.Question, cacheKey);
        if (groqResponse != null)
        {
            return groqResponse;
        }

        logger.LogError("Ambos os provedores (Gemini e Groq) falharam para: {Question}", questionDto.Question);
        return CreateFallbackResponse("all_providers_failed");
    }

    private async Task<ChatbotResponseDto?> TryGeminiAsync(string question, string cacheKey)
    {
        try
        {
            var apiKey = configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                logger.LogWarning("Chave da API do Gemini não configurada, pulando para Groq");
                return null;
            }

            var prompt = BuildPrompt(question);
            var requestBody = CreateGeminiRequestBody(prompt);
            var json = JsonSerializer.Serialize(requestBody,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={apiKey}";

            return await ExecuteGeminiWithRetry(url, content, question, cacheKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao tentar Gemini, fallback para Groq: {Message}", ex.Message);
            return null;
        }
    }

    private async Task<ChatbotResponseDto?> TryGroqAsync(string question, string cacheKey)
    {
        try
        {
            var apiKey = configuration["Groq:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                logger.LogError("Chave da API do Groq não configurada");
                return null;
            }

            var prompt = BuildPrompt(question);
            var requestBody = CreateGroqRequestBody(prompt);
            var json = JsonSerializer.Serialize(requestBody,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var url = "https://api.groq.com/openai/v1/chat/completions";

            return await ExecuteGroqWithRetry(url, content, question, cacheKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao tentar Groq: {Message}", ex.Message);
            return null;
        }
    }

    private async Task<ChatbotResponseDto?> ExecuteGeminiWithRetry(string url, StringContent content, string question, string cacheKey)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Enviando requisição para Gemini (tentativa {Attempt}): {Question}", attempt, question);

                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = ParseGeminiResponse(responseContent);

                    if (result != null)
                    {
                        // Cache com tempo maior para respostas válidas
                        cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheTimeoutMinutes));
                        logger.LogInformation("Resposta bem-sucedida do Gemini para: {Question}", question);
                        return result;
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    logger.LogWarning("Rate limit atingido no Gemini na tentativa {Attempt}", attempt);

                    if (attempt >= MaxRetries)
                    {
                        return null; // Vai tentar Groq
                    }

                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt) * 5);
                    await Task.Delay(delay);
                    continue;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Erro na API do Gemini: {StatusCode} - {Error}", response.StatusCode, errorContent);

                if (attempt >= MaxRetries)
                {
                    return null; // Vai tentar Groq
                }
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries)
            {
                logger.LogWarning(ex, "Erro de conexão no Gemini na tentativa {Attempt}, tentando novamente...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado ao chamar Gemini: {Message}", ex.Message);
                return null; // Vai tentar Groq
            }
        }

        return null; // Falhou no Gemini, vai tentar Groq
    }

    private async Task<ChatbotResponseDto?> ExecuteGroqWithRetry(string url, StringContent content, string question, string cacheKey)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Enviando requisição para Groq (tentativa {Attempt}): {Question}", attempt, question);

                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = ParseGroqResponse(responseContent);

                    if (result != null)
                    {
                        // Cache com tempo maior para respostas válidas
                        cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheTimeoutMinutes));
                        logger.LogInformation("Resposta bem-sucedida do Groq para: {Question}", question);
                        return result;
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    logger.LogWarning("Rate limit atingido no Groq na tentativa {Attempt}", attempt);

                    if (attempt >= MaxRetries)
                    {
                        return null;
                    }

                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt) * 3);
                    await Task.Delay(delay);
                    continue;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Erro na API do Groq: {StatusCode} - {Error}", response.StatusCode, errorContent);

                if (attempt >= MaxRetries)
                {
                    return null;
                }
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries)
            {
                logger.LogWarning(ex, "Erro de conexão no Groq na tentativa {Attempt}, tentando novamente...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado ao chamar Groq: {Message}", ex.Message);
                return null;
            }
        }

        return null;
    }

    private static object CreateGeminiRequestBody(string prompt)
    {
        return new
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
            },
            generationConfig = new
            {
                temperature = 0.3,
                maxOutputTokens = 150,
                topP = 0.8,
                topK = 40
            },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
            }
        };
    }

    private static object CreateGroqRequestBody(string prompt)
    {
        return new
        {
            model = GroqModel,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.3,
            max_tokens = 150,
            top_p = 0.8,
            stream = false
        };
    }

    private static string BuildPrompt(string question)
    {
        return $"""
                Você é Nova, a assistente virtual oficial da NovaBankTech, nosso banco digital. Pode ser chamada carinhosamente de "Nô" ou apenas "Nova".

                💡 Diretrizes de comportamento:
                - Responda somente sobre as funcionalidades e fluxos específicos listados abaixo.
                - Para perguntas sobre economia (taxa Selic, inflação, etc.), diga:  
                  "Para informações econômicas, recomendo consultar fontes oficiais como o Banco Central. Posso ajudá-la com nossos serviços bancários da NovaBankTech!"
                - Se a pergunta fugir do escopo (ex: clima, política, receitas), diga:  
                  "Sou especializada apenas em assuntos da NovaBankTech. Como posso ajudá-la com nossos serviços bancários?"
                - Se mencionarem outros bancos, responda:  
                  "Posso ajudar apenas com serviços da NovaBankTech. Que funcionalidade gostaria de conhecer?"
                - Mantenha respostas objetivas, profissionais e acolhedoras.
                - Ao mencionar a empresa, use sempre: "nossa plataforma", "a NovaBankTech" ou "nossos serviços".
                - Se pedirem suporte humano, informe:  
                  "Você pode falar com nosso time pelo e-mail projetodigitalvault@gmail.com"

                🔧 Funcionalidades e Fluxos Específicos:

                📋 BOLETOS:
                - Gerar boletos: Escolha o valor desejado na função de gerar boletos
                - Pagamento: Digite o número do boleto e senha. Pode pagar parcial ou totalmente
                - Histórico: Visualize boletos pagos ou pendentes, com opção de pagamento para pendentes

                💰 OPERAÇÕES FINANCEIRAS:
                - Histórico de Movimentações: Disponível na tela principal
                - Transferências: Envie dinheiro para outras contas NovaBankTech  
                - Saques e Depósitos: Acesse pela tela inicial
                - PIX: Transferências instantâneas 24h ⚡
                - Consulta de Saldo: Visível no topo da tela inicial 📊
                - Exportar Histórico: Baixe em PDF ou Excel (.xlsx)

                ⚙️ CONFIGURAÇÕES DA CONTA:
                Fluxo correto: Acesse "Configurações" → 3 opções disponíveis:
                1. "Editar Conta" - Para alterar dados pessoais e senha
                2. "Excluir Conta" - Ação irreversível, contate suporte para reativar
                3. "Sair" - Para fazer logout da conta

                🗣️ Pergunta do cliente:
                "{question}"

                🔁 Responda de forma clara, objetiva e direta. **Não utilize formatações como negrito, itálico ou Markdown**. Use apenas texto simples, com emojis sutis quando apropriado. Máximo de 80 palavras. Evite repetições e seja precisa sobre os fluxos do sistema.
                """;
    }

    private static ChatbotResponseDto CreateFallbackResponse(string errorType)
    {
        return errorType switch
        {
            "all_providers_failed" => new ChatbotResponseDto(
                "Estou temporariamente indisponível. Por favor, tente novamente em alguns minutos ou " +
                "entre em contato com nosso suporte: projetodigitalvault@gmail.com 🔧"
            ),
            "rate_limit" => new ChatbotResponseDto(
                "Desculpe, estou temporariamente sobrecarregada. Tente novamente em alguns minutos. " +
                "Posso ajudá-la com pagamentos, transferências, PIX e outras funcionalidades da NovaBankTech! 💳"
            ),
            "connection_error" => new ChatbotResponseDto(
                "Estou com problemas de conectividade. Tente novamente em alguns minutos ou " +
                "entre em contato com nosso suporte: projetodigitalvault@gmail.com 📞"
            ),
            "api_error" => new ChatbotResponseDto(
                "Temporariamente indisponível. Tente novamente em alguns minutos ou " +
                "contate nosso suporte: projetodigitalvault@gmail.com 🔧"
            ),
            "unexpected_error" => new ChatbotResponseDto(
                "Ocorreu um erro inesperado. Por favor, reformule sua pergunta ou " +
                "contate nosso suporte: projetodigitalvault@gmail.com ⚠️"
            ),
            _ => new ChatbotResponseDto(
                "Olá! Sou a Nova, assistente da NovaBankTech. " +
                "Posso ajudá-la com pagamentos, transferências, PIX, consulta de saldo e outras funcionalidades. " +
                "Como posso ajudá-la hoje? 😊"
            )
        };
    }

    private ChatbotResponseDto? ParseGeminiResponse(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() <= 0)
            {
                logger.LogWarning("Resposta do Gemini não contém candidates válidos");
                return null;
            }

            var firstCandidate = candidates[0];

            // Verifica se foi bloqueado por safety
            if (firstCandidate.TryGetProperty("finishReason", out var finishReason) &&
                finishReason.GetString() == "SAFETY")
            {
                logger.LogWarning("Resposta bloqueada por safety settings no Gemini");
                return new ChatbotResponseDto(
                    "Não posso responder essa pergunta. Como posso ajudá-la com nossos serviços bancários? 🏦");
            }

            if (!firstCandidate.TryGetProperty("content", out var contentProp) ||
                !contentProp.TryGetProperty("parts", out var parts) ||
                parts.GetArrayLength() <= 0)
            {
                logger.LogWarning("Estrutura de resposta do Gemini inválida");
                return null;
            }

            var firstPart = parts[0];
            if (!firstPart.TryGetProperty("text", out var text))
            {
                logger.LogWarning("Texto não encontrado na resposta do Gemini");
                return null;
            }

            var responseText = text.GetString()?.Trim();
            if (string.IsNullOrEmpty(responseText))
            {
                logger.LogWarning("Resposta do Gemini estava vazia");
                return null;
            }

            return new ChatbotResponseDto(responseText);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Erro ao fazer parse da resposta do Gemini");
            return null;
        }
    }

    private ChatbotResponseDto? ParseGroqResponse(string responseContent)
    {
        try
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            if (!root.TryGetProperty("choices", out var choices) ||
                choices.GetArrayLength() <= 0)
            {
                logger.LogWarning("Resposta do Groq não contém choices válidos");
                return null;
            }

            var firstChoice = choices[0];

            if (!firstChoice.TryGetProperty("message", out var message) ||
                !message.TryGetProperty("content", out var content))
            {
                logger.LogWarning("Estrutura de resposta do Groq inválida");
                return null;
            }

            var responseText = content.GetString()?.Trim();
            if (string.IsNullOrEmpty(responseText))
            {
                logger.LogWarning("Resposta do Groq estava vazia");
                return null;
            }

            return new ChatbotResponseDto(responseText);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Erro ao fazer parse da resposta do Groq");
            return null;
        }
    }
}