using Newtonsoft.Json;
using Payments.Api.Models;
using Payments.Api.DTOS;

namespace Payments.Api.Clients;

public class BankApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
{
    private void AddAuthorizationHeader()
    {
        var token = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrEmpty(token) && !httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", token);
        }
    }

    public async Task<decimal> GetBalanceAsync()
    {
        Console.WriteLine($"DEBUG: Starting GetBalanceAsync with working URL");

        var token = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        Console.WriteLine($"DEBUG: Token: '{token}'");

        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("JWT Token não encontrado");
        }

        const string url = "http://apigateway:8080/user/api/GetAccountByNumber";

        Console.WriteLine($"DEBUG: Using working URL: {url}");

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", token);

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"DEBUG: Status: {response.StatusCode}");
        Console.WriteLine($"DEBUG: Response: {content}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API Error: {response.StatusCode} - {content}");
        }

        var responseData = JsonConvert.DeserializeObject<ResponseModel<AccountResponseDto>>(content);

        if (responseData?.Data is null)
        {
            throw new Exception($"Invalid response format: {content}");
        }

        Console.WriteLine($"DEBUG: SUCCESS! Balance: {responseData.Data.Balance}");
        return responseData.Data.Balance;
    }


    public async Task<(bool success, string errorMessage)> DebitFromAccountAsyncWithError(DebitPaymentDto dto)
    {
        try
        {
            AddAuthorizationHeader();

            const string url = "http://apigateway:8080/movimentation/api/MakeDebitPayment";

            var response = await httpClient.PostAsJsonAsync(url, dto);

            Console.WriteLine($"DEBUG: Debit API Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"DEBUG: Debit API succeeded");
                return (true, string.Empty);
            }

            // ✅ CAPTURAR MENSAGEM DE ERRO ESPECÍFICA DA API
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"DEBUG: Debit API error content: {errorContent}");

            try
            {
                // Tentar extrair mensagem específica do JSON de erro
                var errorResponse = JsonConvert.DeserializeObject<ResponseModel<bool>>(errorContent);

                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    Console.WriteLine($"DEBUG: Extracted error message: {errorResponse.Message}");
                    return (false, errorResponse.Message);
                }
            }
            catch (Exception parseEx)
            {
                Console.WriteLine($"DEBUG: Failed to parse error response: {parseEx.Message}");
            }

            // Fallback para casos onde não consegue extrair a mensagem
            return (false, $"Erro na operação de débito: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: DebitFromAccountAsyncWithError exception: {ex.Message}");
            return (false, "Erro de comunicação com o servidor. Tente novamente.");
        }
    }

// ✅ MANTER O MÉTODO ORIGINAL PARA COMPATIBILIDADE
    public async Task<bool> DebitFromAccountAsync(DebitPaymentDto dto)
    {
        var (success, _) = await DebitFromAccountAsyncWithError(dto);
        return success;
    }

    public async Task<AccountResponseDto?> GetUserDataByAccountAsync(string accountNumber)
    {
        try
        {
            AddAuthorizationHeader();

            // URL correta para buscar dados da conta
            const string url = "http://apigateway:8080/user/api/GetAccountByNumber";

            Console.WriteLine($"DEBUG: Calling GetUserDataByAccountAsync for account: {accountNumber}");

            var response = await httpClient.GetAsync(url);

            Console.WriteLine($"DEBUG: Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"DEBUG: Failed to get user data: {response.ReasonPhrase}");
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"DEBUG: Response content: {jsonResponse}");

            var responseData = JsonConvert.DeserializeObject<ResponseModel<AccountResponseDto>>(jsonResponse);

            if (responseData?.Data != null)
            {
                Console.WriteLine($"DEBUG: Found user: {responseData.Data.Name}");
                return responseData.Data;
            }

            Console.WriteLine($"DEBUG: No user data in response");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG ERROR: GetUserDataByAccountAsync failed: {ex.Message}");
            return null; // Se der erro, retorna null (tratado no service)
        }
    }

    public async Task<ResponseModel<PaymentResultDto>> ProcessPaymentAsync(PaymentDto dto)
    {
        try
        {
            AddAuthorizationHeader();

            const string url = "http://apigateway:8080/movimentation/api/ProcessPayment";

            Console.WriteLine(
                $"DEBUG BANK CLIENT: Calling ProcessPayment - Account: {dto.AccountNumber}, Value: {dto.Value:C}");

            var response = await httpClient.PostAsJsonAsync(url, dto);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"DEBUG BANK CLIENT: Response Status: {response.StatusCode}");
            Console.WriteLine($"DEBUG BANK CLIENT: Response Content: {content}");

            // ✅ SEMPRE PARSEAR A RESPOSTA PRIMEIRO
            var responseData = JsonConvert.DeserializeObject<ResponseModel<PaymentResultDto>>(content);
            Console.WriteLine($"DEBUG BANK CLIENT: Procedure result - Success: {responseData?.Data?.IsSuccess}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"DEBUG BANK CLIENT: API call failed");

                // ✅ RETORNAR A MENSAGEM REAL DA BANK.API
                return new ResponseModel<PaymentResultDto>
                {
                    Data = responseData?.Data ?? new PaymentResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API Error: {response.StatusCode}"
                    },
                    Message = responseData?.Message ?? "Erro na comunicação com Bank.Api"
                };
            }

            return responseData ?? new ResponseModel<PaymentResultDto>
            {
                Data = new PaymentResultDto { IsSuccess = false, ErrorMessage = "Invalid response format" },
                Message = "Erro no formato da resposta"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG BANK CLIENT ERROR: {ex.Message}");
            return new ResponseModel<PaymentResultDto>
            {
                Data = new PaymentResultDto { IsSuccess = false, ErrorMessage = ex.Message },
                Message = "Erro de comunicação com Bank.Api"
            };
        }
    }
}