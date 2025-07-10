using Bank.Api.DTOS;
using Bank.Api.Utils;
using User.Api.Model;

namespace Bank.Api.Services.PixServices
{
    public class PixService : IPixService
    {
        private readonly PixClient _client;

        public PixService(PixClient client)
        {
            _client = client;
        }

        public async Task<ResponseModel<string>> CriarPix(RegistroPixDto data)
        {
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
