using Auth.Api.Dtos;
using Auth.Api.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseModel<LoginResponseDto>> AuthenticateAsync(LoginRequestDto dto)
        {
            var response = new ResponseModel<LoginResponseDto>();
            try
            {
                var client = _httpClientFactory.CreateClient();
                string baseUrl = _configuration["UserApi:BaseUrl"];
                string url = null;
                bool loginPorCpfOuEmail = false; 
                
                if (!string.IsNullOrWhiteSpace(dto.AccountNumber))
                {
                    url = $"{baseUrl}/GetAccountByLogin/{Uri.EscapeDataString(dto.AccountNumber.Trim())}";
                }
                else if (!string.IsNullOrWhiteSpace(dto.Cpf))
                {
                    url = $"{baseUrl}/GetAccountByCpf/{Uri.EscapeDataString(dto.Cpf.Trim())}";
                }
                else if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    url = $"{baseUrl}/GetAccountByEmail/{Uri.EscapeDataString(dto.Email.Trim())}";
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
                dynamic result = JsonConvert.DeserializeObject(accountJson);
                string userIdStr = result.data.userId;
                string senhaHash = result.data.senhaHash;
                bool accountStatus = result.data.status;
                string accountNumberParaToken = null; 

                if (loginPorCpfOuEmail)
                {
                    List<string> contasDoUsuario = JsonConvert.DeserializeObject<List<string>>(result.data.accountNumbers.ToString());

                    if (contasDoUsuario == null || !contasDoUsuario.Any())
                    {
                        response.Message = "Nenhuma conta encontrada para o CPF/E-mail informado.";
                        response.IsSuccess = false;
                        return response;
                    }

                    if (contasDoUsuario.Count > 1 && string.IsNullOrWhiteSpace(dto.SelectedAccountNumber))
                    {
                        response.Message = "Múltiplas contas encontradas. Por favor, selecione uma.";
                        response.IsSuccess = false;
                        response.Data = new LoginResponseDto { AccountNumbers = contasDoUsuario };
                        return response;
                    }
                    else if (!string.IsNullOrWhiteSpace(dto.SelectedAccountNumber))
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
                    accountNumberParaToken = result.data.accountNumber;
                }

                bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Password, senhaHash);
                if (!senhaValida)
                {
                    response.Message = "Não foi possível realizar o login. Verifique seus dados e tente novamente.";
                    response.IsSuccess = false;
                    return response;
                }

                if (!accountStatus)
                {
                    response.Message = "Não foi possível realizar login, sua conta está desativada, contate a Administração.";
                    response.IsSuccess = false;
                    return response;
                }

                var claims = new List<Claim>
                {
                    new Claim("AccountNumber", accountNumberParaToken),
                    new Claim("UserId", userIdStr)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // Atualiza token no User API
                var updateUrl = $"{baseUrl}/update-token/{dto.AccountNumber}";
                var updateDto = new { token = tokenString };
                var json = JsonConvert.SerializeObject(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var saveTokenResponse = await client.PutAsync(updateUrl, content);

                if (!saveTokenResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro ao salvar token no User API. Status: {saveTokenResponse.StatusCode}");
                }

                response.Data = new LoginResponseDto { Token = tokenString };
                response.Message = "Usuário autenticado com sucesso!";
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Erro ao autenticar usuário: {ex.Message}";
                return response;
            }
        }

    }
}