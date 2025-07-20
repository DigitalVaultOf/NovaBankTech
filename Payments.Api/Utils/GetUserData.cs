// Payments.Api/Utils/GetUserData.cs
// VERSÃO MAIS CONCISA (se preferir menos código)

using Payments.Api.Clients;
using System.Security.Claims;

namespace Payments.Api.Utils;

public class GetUserData(IHttpContextAccessor httpContextAccessor, BankApiClient bankApiClient)
{
    private static readonly string[] UserNameClaims =
    [
        "Name", "name", "UserName", "given_name", ClaimTypes.Name, ClaimTypes.GivenName
    ];

    public Guid GetCurrentUserId()
    {
        var userIdClaim = GetClaim("UserId") ?? GetClaim(ClaimTypes.NameIdentifier);
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            Log($"UserId: {userId}");
            return userId;
        }

        LogClaims();
        throw new UnauthorizedAccessException("UserId não encontrado no token.");
    }

    public string GetCurrentAccountNumber()
    {
        var accountNumber = GetClaim("AccountNumber");
        
        if (string.IsNullOrEmpty(accountNumber))
        {
            LogClaims();
            throw new UnauthorizedAccessException("AccountNumber não encontrado no token.");
        }

        Log($"AccountNumber: {accountNumber}");
        return accountNumber;
    }

    public string GetCurrentUserName()
    {
        try
        {
            var userName = GetUserNameFromClaims();
            if (!string.IsNullOrEmpty(userName))
            {
                Log($"UserName from JWT: {userName}");
                return userName;
            }

            var accountNumber = GetCurrentAccountNumber();
            Log($"UserName fallback: Conta {accountNumber}");
            return $"Conta {accountNumber}";
        }
        catch (Exception ex)
        {
            Log($"GetCurrentUserName error: {ex.Message}");
            return "Usuário";
        }
    }

    public async Task<string> GetUserNameForBoletoAsync(string accountNumber)
    {
        try
        {
            Log($"Getting username for account: {accountNumber}");

            // Se for conta atual, usar JWT
            if (IsCurrentAccount(accountNumber))
            {
                var jwtName = GetUserNameFromClaims();
                if (!string.IsNullOrEmpty(jwtName))
                {
                    Log($"Found in JWT: {jwtName}");
                    return jwtName;
                }
            }

            // Tentar via API
            var userData = await bankApiClient.GetUserDataByAccountAsync(accountNumber);
            if (userData?.Name != null)
            {
                Log($"Found via API: {userData.Name}");
                return userData.Name;
            }

            // Fallback
            Log($"Using fallback for account: {accountNumber}");
            return $"Conta {accountNumber}";
        }
        catch (Exception ex)
        {
            Log($"GetUserNameForBoletoAsync error: {ex.Message}");
            return $"Conta {accountNumber}";
        }
    }

    private string? GetClaim(string type) => 
        httpContextAccessor.HttpContext?.User.FindFirst(type)?.Value;

    private string? GetUserNameFromClaims() => 
        UserNameClaims.Select(GetClaim).FirstOrDefault(name => !string.IsNullOrEmpty(name));

    private bool IsCurrentAccount(string accountNumber)
    {
        try { return GetCurrentAccountNumber() == accountNumber; }
        catch { return false; }
    }

    private static void Log(string message) => 
        Console.WriteLine($"DEBUG GetUserData: {message}");

    private void LogClaims()
    {
        var claims = httpContextAccessor.HttpContext?.User.Claims
            .Select(c => $"{c.Type}:{c.Value}") ?? Enumerable.Empty<string>();
        Log($"Claims: [{string.Join(", ", claims)}]");
    }
}