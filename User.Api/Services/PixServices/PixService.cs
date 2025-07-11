using Bank.Api.DTOS;
using Bank.Api.Utils;
using User.Api.Model;

namespace Bank.Api.Services.PixServices
{
    public class PixService : IPixService
    {
        private readonly PixClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PixService(PixClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseModel<string>> CriarPix(RegistroPixDto data)
        {
            var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;

            data.Account = accountNumber;

            var response = new ResponseModel<string>();
            try
            {
                await _client.CriarPixAsync(data);
                response.Data = "Pix Criado com sucesso";
                return response;

            }
            catch (Exception e)
            {
                response.Message = $"Não foi possivel criar pix: {e.Message}";
                return response;
            }
        }
    }
}
