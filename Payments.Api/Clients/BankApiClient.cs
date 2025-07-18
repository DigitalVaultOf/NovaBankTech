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
        AddAuthorizationHeader();

        const string url = "http://apigateway:8080/user/api/User/GetAccountByNumber";

        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro ao buscar saldo da conta: {response.ReasonPhrase}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var responseData = JsonConvert.DeserializeObject<ResponseModel<AccountResponseDto>>(jsonResponse);

        if (responseData?.Data is null)
        {
            throw new Exception("A resposta da API de banco não continha os dados de saldo esperados.");
        }

        return responseData.Data.Balance;
    }

    public async Task<bool> DebitFromAccountAsync(DebitPaymentDto dto)
    {
        AddAuthorizationHeader();

        const string url = "http://apigateway:8080/movimentation/api/Movimentation/MakeDebitPayment";

        var response = await httpClient.PostAsJsonAsync(url, dto);

        return response.IsSuccessStatusCode;
    }
}