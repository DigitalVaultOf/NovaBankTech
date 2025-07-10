using Pix.Api.DTOS;
using Pix.Api.Models;

namespace Pix.Api.Services.PixService
{
    public interface IPixService
    {
        Task<ResponseModel<string>> RegistroPix(RegistroPixDto data);
    }
}
