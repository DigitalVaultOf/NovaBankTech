using Bank.Api.DTOS;
using User.Api.Model;

namespace Bank.Api.Services.Movimentations
{
    public interface IMovimentationService
    {
        Task<ResponseModel<string>> MovimentationDepositAsync(MovimentationDto data);
        Task<ResponseModel<string>> MovimentationWithdrawAsync(MovimentationDto data);
    }
}
