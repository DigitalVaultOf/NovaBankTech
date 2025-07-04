using Bank.Api.Model;
using Bank.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using User.Api.Data;

namespace Bank.Api.Repositories.Implementations
{
    public class MovementHistoryRepository : IMovementRepository
    {
        private readonly AppDbContext _context;
        public MovementHistoryRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Movimentations>> GetMovimentationsAsync(Guid userId)
        {
            return await _context.Moviments
                .Include(m => m.accountNumber)
                .Where(m => m.accountNumber == userId)
                .OderByDescending(m => m.Date)
                .ToListAsync();

        }
    }
}
