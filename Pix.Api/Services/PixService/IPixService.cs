using Pix.Api.DTOS;
using Pix.Api.Models;

namespace Pix.Api.Services.PixService
{
    public interface IPixService
    {
        Task<ResponseModel<string>> RegistroPix(RegistroPixDto data);
        Task<ResponseModel<string>> RegistroTransferencia(TransferDto data);
        Task<ResponseModel<string>> GetNumberAccountAsync(string chave);
        Task<ResponseModel<bool>> HasPix();
        Task<ResponseModel<string>> getPixKey();
    }
}
