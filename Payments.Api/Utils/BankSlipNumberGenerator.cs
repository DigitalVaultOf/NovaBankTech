namespace Payments.Api.Utils;

public static class BankSlipNumberGenerator
{
    public static long Generate()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomNumber = new Random().Next(10, 99).ToString();
        var bankSlipString = timestamp + randomNumber;
        return long.Parse(bankSlipString);
    }
}