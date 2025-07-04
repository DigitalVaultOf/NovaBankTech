using Bank.Api.Model;

namespace Bank.Api.Repositories.Interfaces
{
    public interface IMovementRepository
    {
        Task<IEnumerable<Movimentations>>GetMovimentationsAsync(Guid userId);

    }
}
