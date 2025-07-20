using System.Globalization;

namespace Payments.Api.Utils;

/// <summary>
/// Gerenciador de números de boleto bancário brasileiro
/// Formato: ddMMyyyyHHmmssfffR (18 dígitos)
/// - dd: Dia (01-31)
/// - MM: Mês (01-12) 
/// - yyyy: Ano (2020-2050)
/// - HH: Hora (00-23)
/// - mm: Minutos (00-59)
/// - ss: Segundos (00-59)
/// - fff: Milissegundos (000-999)
/// - R: Random (0-9)
/// </summary>
public static class BankSlipNumberManager
{
    private static readonly Random Random = new();

    /// <summary>
    /// Gera um novo número de boleto baseado no horário brasileiro atual
    /// </summary>
    /// <returns>Número do boleto com 18 dígitos</returns>
    public static long Generate()
    {
        // ✅ HORÁRIO DO BRASIL (UTC-3)
        var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        var brazilTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);

        Console.WriteLine($"DEBUG BankSlipNumberManager: Brazil Time: {brazilTime:dd/MM/yyyy HH:mm:ss.fff}");

        // ✅ GERAR RANDOM DE 1 DÍGITO (0-9)
        var randomDigit = Random.Next(0, 10);

        // ✅ FORMATO: ddMMyyyyHHmmssfffR (18 dígitos)
        var bankSlipString = $"{brazilTime:ddMMyyyy}{brazilTime:HHmmss}{brazilTime:fff}{randomDigit}";

        Console.WriteLine($"DEBUG BankSlipNumberManager: Generated: {brazilTime:ddMMyyyy} {brazilTime:HH:mm:ss.fff}+{randomDigit} = {bankSlipString}");
        Console.WriteLine($"DEBUG BankSlipNumberManager: Length: {bankSlipString.Length} digits");

        return long.Parse(bankSlipString);
    }
    
    /// <summary>
    /// Converte long para string
    /// </summary>
    public static string ConvertToString(long bankSlipNumber)
    {
        return bankSlipNumber.ToString();
    }

    /// <summary>
    /// Converte string para long com validação de formato
    /// </summary>
    public static long? ConvertToLong(string bankSlipNumber)
    {
        Console.WriteLine($"DEBUG BankSlipNumberManager: Converting BankSlipNumber: {bankSlipNumber}");

        if (string.IsNullOrWhiteSpace(bankSlipNumber))
        {
            Console.WriteLine($"DEBUG BankSlipNumberManager: BankSlipNumber is null or empty");
            return null;
        }

        // ✅ TENTAR CONVERTER PARA LONG
        if (!long.TryParse(bankSlipNumber.Trim(), out var bankSlipNumberLong))
        {
            Console.WriteLine($"DEBUG BankSlipNumberManager: Failed to parse BankSlipNumber to long");
            return null;
        }

        // ✅ VALIDAR FORMATO BRASILEIRO (18 DÍGITOS)
        if (!IsValidFormat(bankSlipNumberLong))
        {
            Console.WriteLine($"DEBUG BankSlipNumberManager: BankSlipNumber doesn't match Brazilian format: {bankSlipNumber}");
            return null;
        }

        Console.WriteLine($"DEBUG BankSlipNumberManager: Successfully converted: {bankSlipNumberLong}");
        return bankSlipNumberLong;
    }

    /// <summary>
    /// Valida se long segue o formato brasileiro ATUALIZADO (18 dígitos)
    /// </summary>
    public static bool IsValidFormat(long bankSlipNumber)
    {
        var bankSlipStr = bankSlipNumber.ToString();

        // ✅ CORRIGIDO: Deve ter exatamente 18 dígitos (não 16!)
        if (bankSlipStr.Length != 18)
            return false;

        // ✅ VALIDAÇÕES DO FORMATO 18 DÍGITOS: ddMMyyyyHHmmssfffR
        
        // Validar data (primeiros 8 dígitos): ddMMyyyy
        if (!int.TryParse(bankSlipStr[..2], out var day) || day < 1 || day > 31)
            return false;

        if (!int.TryParse(bankSlipStr[2..4], out var month) || month < 1 || month > 12)
            return false;

        if (!int.TryParse(bankSlipStr[4..8], out var year) || year < 2020 || year > 2050)
            return false;

        // Validar hora (próximos 6 dígitos): HHmmss  
        if (!int.TryParse(bankSlipStr[8..10], out var hour) || hour < 0 || hour > 23)
            return false;

        if (!int.TryParse(bankSlipStr[10..12], out var minute) || minute < 0 || minute > 59)
            return false;

        if (!int.TryParse(bankSlipStr[12..14], out var second) || second < 0 || second > 59)
            return false;

        // Validar milissegundos (próximos 3 dígitos): fff
        if (!int.TryParse(bankSlipStr[14..17], out var milliseconds) || milliseconds < 0 || milliseconds > 999)
            return false;

        // Validar random final (último dígito): R
        if (!int.TryParse(bankSlipStr[17..18], out var random) || random < 0 || random > 9)
            return false;

        return true;
    }

    /// <summary>
    /// Formata BankSlipNumber para exibição brasileira legível (18 dígitos)
    /// </summary>
    public static string FormatForDisplay(long bankSlipNumber)
    {
        var bankSlipStr = bankSlipNumber.ToString();

        // ✅ CORRIGIDO: Formato 18 dígitos
        if (bankSlipStr.Length == 18)
        {
            var day = bankSlipStr[..2];
            var month = bankSlipStr[2..4];
            var year = bankSlipStr[4..8];
            var hour = bankSlipStr[8..10];
            var minute = bankSlipStr[10..12];
            var second = bankSlipStr[12..14];
            var milliseconds = bankSlipStr[14..17];
            var random = bankSlipStr[17..18];

            return $"{day}/{month}/{year} {hour}:{minute}:{second}.{milliseconds}+{random}";
        }

        return bankSlipStr; // Fallback
    }

    /// <summary>
    /// Extrai a data do BankSlipNumber brasileiro (18 dígitos)
    /// </summary>
    public static DateTime? ExtractDate(long bankSlipNumber)
    {
        try
        {
            var bankSlipStr = bankSlipNumber.ToString();

            // ✅ CORRIGIDO: 18 dígitos
            if (bankSlipStr.Length != 18)
                return null;

            var day = int.Parse(bankSlipStr[..2]);
            var month = int.Parse(bankSlipStr[2..4]);
            var year = int.Parse(bankSlipStr[4..8]);
            var hour = int.Parse(bankSlipStr[8..10]);
            var minute = int.Parse(bankSlipStr[10..12]);
            var second = int.Parse(bankSlipStr[12..14]);
            var milliseconds = int.Parse(bankSlipStr[14..17]);

            return new DateTime(year, month, day, hour, minute, second, milliseconds);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extrai apenas a data (sem hora) do BankSlipNumber
    /// </summary>
    public static DateTime? ExtractDateOnly(long bankSlipNumber)
    {
        var fullDate = ExtractDate(bankSlipNumber);
        return fullDate?.Date;
    }

    /// <summary>
    /// Verifica se o boleto foi gerado hoje
    /// </summary>
    public static bool IsGeneratedToday(long bankSlipNumber)
    {
        var boletoDate = ExtractDateOnly(bankSlipNumber);
        if (boletoDate == null) return false;

        var today = DateTime.Today;
        return boletoDate.Value.Date == today;
    }

    /// <summary>
    /// Verifica se o boleto foi gerado em uma data específica
    /// </summary>
    public static bool IsGeneratedOn(long bankSlipNumber, DateTime targetDate)
    {
        var boletoDate = ExtractDateOnly(bankSlipNumber);
        if (boletoDate == null) return false;

        return boletoDate.Value.Date == targetDate.Date;
    }

    /// <summary>
    /// Calcula quantos dias se passaram desde a geração do boleto
    /// </summary>
    public static int? DaysSinceGeneration(long bankSlipNumber)
    {
        var boletoDate = ExtractDateOnly(bankSlipNumber);
        if (boletoDate == null) return null;

        return (DateTime.Today - boletoDate.Value.Date).Days;
    }

    /// <summary>
    /// Exemplo de uso e validação
    /// </summary>
    public static void ExampleUsage()
    {
        Console.WriteLine("=== EXEMPLO DE USO ===");
        
        // Gerar novo boleto
        var novoBoleto = Generate();
        Console.WriteLine($"Novo boleto: {novoBoleto}");
        Console.WriteLine($"Formatado: {FormatForDisplay(novoBoleto)}");
        
        // Validar formato
        var isValid = IsValidFormat(novoBoleto);
        Console.WriteLine($"Formato válido: {isValid}");
        
        // Extrair data
        var dataGeracao = ExtractDate(novoBoleto);
        Console.WriteLine($"Data de geração: {dataGeracao}");
        
        // Verificar se foi gerado hoje
        var foiGeradoHoje = IsGeneratedToday(novoBoleto);
        Console.WriteLine($"Foi gerado hoje: {foiGeradoHoje}");
        
        Console.WriteLine("======================");
    }
}