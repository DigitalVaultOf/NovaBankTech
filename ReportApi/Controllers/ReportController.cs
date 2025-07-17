using Microsoft.AspNetCore.Mvc;
using ReportApi.Services.ReportService;

namespace ReportApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("csv")]
    public async Task<IActionResult> GetCsv()
    {
        try
        {
            var (fileBytes, fileName, contentType) = await _reportService.GenerateCsvReportAsync();
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                mensagem = "Erro ao gerar CSV",
                detalhe = ex.Message,
                pilha = ex.StackTrace
            });
        }
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> GetPdf()
    {
        try
        {
            var (fileBytes, fileName, contentType) = await _reportService.GeneratePdfReportAsync();
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                mensagem = "Erro ao gerar PDF",
                detalhe = ex.Message,
                pilha = ex.StackTrace
            });
        }
    }
}
