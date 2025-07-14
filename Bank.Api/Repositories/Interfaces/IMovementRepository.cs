using Bank.Api.DTOS;
using Bank.Api.Model;
using User.Api.Model;

namespace Bank.Api.Repositories.Interfaces
{
    public interface IMovementRepository
    {
        Task<ResponseModel<List<MovimentHistoryDto>>> GetMovimentationsAsync();

    }
}
