using System.Threading.Tasks;

namespace ReportApi.Services.ReportService;

public interface IReportService
{
    Task<(byte[] fileBytes, string fileName, string contentType)> GenerateCsvReportAsync();
    Task<(byte[] fileBytes, string fileName, string contentType)> GeneratePdfReportAsync();
}
