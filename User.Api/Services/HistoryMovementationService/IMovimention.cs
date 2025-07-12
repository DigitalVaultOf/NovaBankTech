using Bank.Api.DTOS;
using User.Api.Model;

namespace Bank.Api.Services.HistoryMovementationService
{
    public interface IMovimention
    {
        Task<ResponseModel<List<MovimentHistoryDto>>> GetMovimentationsAsync();
        Task<ResponseModel<List<MovimentHistoryDto>>> GetMovimentations1WeakAsync();
        Task<ResponseModel<List<MovimentHistoryDto>>> GetMovimentations1MounthAsync();
        Task<ResponseModel<PagesOfMovimentHistoryDto<MovimentHistoryDto>>> GetPagedMovimentationsAsync(MovimentRequestDto pagination);   
    }
}
