using System.Net.Mime;
using System.Text;
using System.Text.Json;
using ReportApi.Model;

namespace ReportApi.Services.ReportService;

public class ReportService : IReportService
{
    private readonly HttpClient _httpClient;
    private const string MovimentationApiUrl = "http://user:8080/api/Movimentation/listmovimentation";

    public ReportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(byte[] fileBytes, string fileName, string contentType)> GenerateCsvReportAsync()
    {
        List<Report>? movimentations = null;

        try
        {
            Console.WriteLine("Buscando dados da API de movimentações...");

            var response = await _httpClient.GetAsync(MovimentationApiUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Resposta JSON da API: " + json);

            movimentations = JsonSerializer.Deserialize<List<Report>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Console.WriteLine($"Foram obtidas {movimentations?.Count ?? 0} movimentações.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter dados da API: " + ex.Message);
            throw new Exception("Erro ao obter dados da API de movimentações: " + ex.Message);
        }

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

        return (fileBytes, fileName, System.Net.Mime.MediaTypeNames.Text.Csv);
    }

    public Task<(byte[] fileBytes, string fileName, string contentType)> GeneratePdfReportAsync()
    {
        throw new NotImplementedException("Geração PDF ainda não implementada.");
    }
}
