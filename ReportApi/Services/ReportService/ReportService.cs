using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ReportApi.Model;

namespace ReportApi.Services.ReportService;

public class ReportService : IReportService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string MovimentationApiUrl = "http://user:8080/api/Movimentation/listmovimentation";

    public ReportService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(byte[] fileBytes, string fileName, string contentType)> GenerateCsvReportAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var accountNumber = user?.FindFirst("AccountNumber")?.Value;

        if (string.IsNullOrEmpty(accountNumber))
            throw new Exception("AccountNumber não encontrado no token.");

        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Token não encontrado no cabeçalho da requisição.");

        var request = new HttpRequestMessage(HttpMethod.Get, MovimentationApiUrl);
        request.Headers.Add("Authorization", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var movimentationResponse = JsonSerializer.Deserialize<MovimentationListResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var movimentations = movimentationResponse?.Data;

        if (movimentations == null || movimentations.Count == 0)
            throw new Exception("Nenhuma movimentação encontrada.");

        var sb = new StringBuilder();
        sb.AppendLine("NumeroContaDestino,TipoMovimento,DataMovimento,Valor,NumeroContaOrigem");

        foreach (var m in movimentations)
        {
            var accountTo = m.AcountNumberTo ?? "";
            sb.AppendLine($"{accountTo},{m.MovimentTypeEnum},{m.DateTimeMoviment:yyyy-MM-dd HH:mm:ss},{m.Amount},{m.AcountNumber}");
        }

        var fileBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"relatorio_{DateTime.Now:yyyyMMddHHmmss}.csv";

        return (fileBytes, fileName, MediaTypeNames.Text.Plain);
    }

    public Task<(byte[] fileBytes, string fileName, string contentType)> GeneratePdfReportAsync()
    {
        throw new NotImplementedException();
    }
}
