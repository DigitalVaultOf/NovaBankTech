using Auth.Api.Dtos;
using Auth.Api.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Api.Services;

public class AuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    : IAuthService
{
    public async Task<ResponseModel<LoginResponseDto>> AuthenticateAsync(LoginRequestDto dto)
    {
        var response = new ResponseModel<LoginResponseDto>();
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                response.IsSuccess = false;
                response.Message = "Senha é obrigatória.";
                return response;
            }

            var client = httpClientFactory.CreateClient();
            var baseUrl = configuration["UserApi:BaseUrl"];
            string? url;
            var loginPorCpfOuEmail = false;

            if (!string.IsNullOrWhiteSpace(dto.AccountNumber))
            {
                url = $"{baseUrl}/GetAccountByLogin/{Uri.EscapeDataString(dto.AccountNumber.Trim())}";
            }
            else if (!string.IsNullOrWhiteSpace(dto.Cpf))
            {
                url = $"{baseUrl}/GetAccountByCpf/{Uri.EscapeDataString(dto.Cpf.Trim())}";
                loginPorCpfOuEmail = true;
            }
            else if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                url = $"{baseUrl}/GetAccountByEmail/{Uri.EscapeDataString(dto.Email.Trim())}";
                loginPorCpfOuEmail = true;
            }
            else
            {
                response.Message = "É necessário informar AccountNumber, CPF ou Email.";
                response.IsSuccess = false;
                return response;
            }

            var userResponse = await client.GetAsync(url);

            if (!userResponse.IsSuccessStatusCode)
            {
                response.Message = "Erro ao autenticar usuário.";
                response.IsSuccess = false;
                return response;
            }

            var accountJson = await userResponse.Content.ReadAsStringAsync();
            dynamic? result = JsonConvert.DeserializeObject(accountJson);

            if (result?.data == null)
            {
                response.Message = "Dados de usuário não encontrados ou formato inválido na resposta da API.";
                response.IsSuccess = false;
                return response;
            }

            string userIdStr = result.data.userId;
            string senhaHash = result.data.senhaHash;
            bool accountStatus = result.data.status;

            var validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, senhaHash);

            if (!validPassword)
            {
                response.Message = "Não foi possível realizar o login. Verifique seus dados e tente novamente.";
                response.IsSuccess = false;
                return response;
            }

            if (!accountStatus)
            {
                response.Message =
                    "Não foi possível realizar login, sua conta está desativada, contate a Administração.";
                response.IsSuccess = false;
                return response;
            }

            string? accountNumberParaToken;


            if (loginPorCpfOuEmail)
            {
                if (result.data.accountNumbers == null)
                {
                    response.Message = "A API não retornou as contas associadas a este CPF/E-mail.";
                    response.IsSuccess = false;
                    return response;
                }

                List<string> contasDoUsuario =
                    JsonConvert.DeserializeObject<List<string>>(result.data.accountNumbers.ToString());

                if (contasDoUsuario == null || !contasDoUsuario.Any())
                {
                    response.Message = "Nenhuma conta encontrada para o CPF/E-mail informado.";
                    response.IsSuccess = false;
                    return response;
                }

                if (contasDoUsuario.Count > 1 && string.IsNullOrWhiteSpace(dto.SelectedAccountNumber))
                {
                    response.Message = "Múltiplas contas encontradas. Por favor, selecione uma.";
                    response.IsSuccess = true;
                    response.Data = new LoginResponseDto { AccountNumbers = contasDoUsuario };
                    return response;
                }

                if (!string.IsNullOrWhiteSpace(dto.SelectedAccountNumber))
                {
                    if (!contasDoUsuario.Contains(dto.SelectedAccountNumber))
                    {
                        response.Message = "O número de conta selecionado não está associado a este CPF/E-mail.";
                        response.IsSuccess = false;
                        return response;
                    }

                    accountNumberParaToken = dto.SelectedAccountNumber;
                }
                else
                {
                    accountNumberParaToken = contasDoUsuario.First();
                }
            }
            else
            {
                if (result.data.accountNumber == null)
                {
                    response.Message = "A API não retornou o número da conta para este login.";
                    response.IsSuccess = false;
                    return response;
                }

                accountNumberParaToken = result.data.accountNumber;
            }

            var claims = new List<Claim>
            {
                new("AccountNumber", accountNumberParaToken),
                new("UserId", userIdStr)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var updateUrl = $"{baseUrl}/update-token/{accountNumberParaToken}";
            var updateDto = new { token = tokenString };
            var json = JsonConvert.SerializeObject(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var saveTokenResponse = await client.PutAsync(updateUrl, content);

            if (!saveTokenResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro ao salvar token no User API. Status: {saveTokenResponse.StatusCode}");
            }

            response.Data = new LoginResponseDto
            {
                Token = tokenString,
                AccountNumbers = loginPorCpfOuEmail && result.data.accountNumbers != null
                    ? JsonConvert.DeserializeObject<List<string>>(result.data.accountNumbers.ToString())
                    : null
            };
            response.Message = "Usuário autenticado com sucesso!";
            response.IsSuccess = true;
            return response;
        }
        catch (Exception ex)
        {
            response.Message = $"Erro ao autenticar usuário: {ex.Message}";
            return response;
        }
    }
}