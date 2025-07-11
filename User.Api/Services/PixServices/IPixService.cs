using Bank.Api.DTOS;
using User.Api.Model;

namespace Bank.Api.Services.PixServices
{
    public interface IPixService
    {
        Task<ResponseModel<string>> CriarPix(RegistroPixDto data);
        Task<ResponseModel<string>> GetAccount(string chave);
        Task<ResponseModel<string>> MakeTransfer(MakePixDto data);
    }
}
