using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Auth.Api.Middlewares;

public class TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // ✅ Pular validação para rotas públicas (login, swagger, etc.)
        if (ShouldSkipValidation(context.Request.Path))
        {
            await next(context);
            return;
        }

        // ✅ Verificar se tem header de Authorization
        if (context.Request.Headers.TryGetValue("Authorization", out var value))
        {
            var authHeader = value.FirstOrDefault();
            
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    
                    // ✅ Verificar se é um JWT válido
                    if (!handler.CanReadToken(token))
                    {
                        logger.LogWarning("🚨 Token inválido recebido no Auth.Api");
                        await WriteErrorResponse(context, "Token inválido. Por favor, faça login novamente.", "INVALID_TOKEN");
                        return;
                    }

                    var jsonToken = handler.ReadJwtToken(token);
                    
                    // ✅ Verificar se token está expirado
                    if (jsonToken.ValidTo < DateTime.UtcNow)
                    {
                        logger.LogInformation("🕒 Token expirado detectado no Auth.Api. Expiration: {Expiration}, Now: {Now}", 
                            jsonToken.ValidTo, DateTime.UtcNow);
                        
                        await WriteErrorResponse(context, "Sua sessão expirou. Por favor, faça login novamente.", "TOKEN_EXPIRED");
                        return;
                    }

                    // ✅ Log se token expira em breve
                    var minutesUntilExpiration = (jsonToken.ValidTo - DateTime.UtcNow).TotalMinutes;
                    if (minutesUntilExpiration is <= 5 and > 0)
                    {
                        logger.LogInformation("⚠️ Token do Auth.Api expirará em {Minutes} minutos", Math.Round(minutesUntilExpiration, 1));
                    }

                    logger.LogDebug("✅ Token válido no Auth.Api. Expira em: {Expiration}", jsonToken.ValidTo);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ Erro ao validar token no Auth.Api");
                    await WriteErrorResponse(context, "Token inválido. Por favor, faça login novamente.", "TOKEN_VALIDATION_ERROR");
                    return;
                }
            }
        }
        
        // ✅ Continuar para o próximo middleware
        await next(context);
    }

    /// <summary>
    /// Define quais rotas do Auth Api não precisam de validação de token
    /// </summary>
    private static bool ShouldSkipValidation(PathString path)
    {
        var skipPaths = new[]
        {
            "/swagger",
            "/api/login",           // ✅ Rota de login deve ser livre
            "/api/register",        // ✅ Se tiver registro
            "/health",
            "/favicon.ico",
            "/"                     // ✅ Rota raiz
        };

        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Resposta de erro padronizada para o Auth Api
    /// </summary>
    private static async Task WriteErrorResponse(HttpContext context, string message, string errorCode)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        
        var errorResponse = new 
        { 
            message,
            error = errorCode,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value,
            microservice = "Auth.Api"  // ✅ Identificar qual microserviço
        };
        
        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await context.Response.WriteAsync(jsonResponse);
    }
}