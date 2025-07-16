using Email.Api.Model;
using Email.Api.Services.RabbitMQServices;
using Email.Api.Services.RegisterEmailServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<RabbitMQConsumer>();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IRegisterEmailService, RegisterEmailService>();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


app.MapGet("/", () => "Email.Api rodando e escutando RabbitMQ!");


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
