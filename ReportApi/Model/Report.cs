namespace ReportApi.Model;

public class Report
{
    public string? AcountNumberTo { get; set; }
    public string MovimentTypeEnum { get; set; }
    public DateTime DateTimeMoviment { get; set; }
    public decimal Amount { get; set; }
    public string? AcountNumber { get; set; }
}
