using System.Text;
using Ai.Api.Services;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configurações básicas
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();

// Configuração de Logging mais detalhada
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuração do HttpClient com timeout otimizado
builder.Services.AddHttpClient<IAiService, AiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60); // Timeout apropriado para IA
});

// Configuração JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? 
    throw new InvalidOperationException("Token JWT não configurado para o serviço de Inteligência Artificial.");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(5) // Tolerância de 5 minutos para sincronização de relógio
        };
        
        // Eventos para logging de autenticação
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Falha na autenticação JWT: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogDebug("Token JWT validado com sucesso");
                return Task.CompletedTask;
            }
        };
    });

// Configuração Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NovaBankTech AI API",
        Version = "v1",
        Description = "API de Inteligência Artificial para a assistente virtual Nova do NovaBankTech",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Suporte NovaBankTech",
            Email = "support@digitalvault.com"
        }
    });

    // Suporte JWT Bearer no Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT no formato: **Bearer {seu token aqui}**"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Registrar serviços
builder.Services.AddScoped<IAiService, AiService>();

var app = builder.Build();

// Configuração do pipeline de middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NovaBankTech AI API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}
else
{
    // Em produção, só habilita Swagger se explicitamente configurado
    var enableSwaggerInProduction = builder.Configuration.GetValue<bool>("Swagger:EnableInProduction");
    if (enableSwaggerInProduction)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "NovaBankTech AI API v1");
        });
    }
}

// Middleware de tratamento de erros global
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError("Erro não tratado na aplicação");
        
        await context.Response.WriteAsync("""
            {
                "error": "Erro interno do servidor",
                "message": "Entre em contato com o suporte: support@digitalvault.com"
            }
            """);
    });
});

// Middleware de segurança
app.UseAuthentication();
app.UseAuthorization();

// Health check básico
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
    .WithTags("Health")
    .AllowAnonymous();

// Mapear controllers
app.MapControllers();

// Configurações de inicialização
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 NovaBankTech AI API iniciada com sucesso!");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();