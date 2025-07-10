using Bank.Api.DTOS;
using User.Api.Model;

namespace Bank.Api.Services.PixServices
{
    public interface IPixService
    {
        Task<ResponseModel<string>> CriarPix(RegistroPixDto data);
    }
}
