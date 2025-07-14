using Bank.Api.DTOS;
using User.Api.Model;

namespace Bank.Api.Services.TransferServices
{
    public interface ITransferService
    {
        Task<ResponseModel<string>> PerformTransferAsync(TransferRequestDTO dto);
    }
}
