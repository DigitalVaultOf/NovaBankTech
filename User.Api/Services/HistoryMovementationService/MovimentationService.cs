using Bank.Api.DTOS;
using Microsoft.EntityFrameworkCore;
using User.Api.Data;
using User.Api.Model;

namespace Bank.Api.Services.HistoryMovementationService
{
    public class MovimentionService : IMovimention
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MovimentionService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
   

        public async Task<ResponseModel<List<MovimentHistoryDto>>> GetMovimentationsAsync()
        {
            var response = new ResponseModel<List<MovimentHistoryDto>>();
            try
            {
                var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;

                var movimentations = await _context.Moviments
                 .Where(m => m.accountNumber == accountNumber)
                 .OrderByDescending(m => m.DateTimeMoviment)
                 .ToListAsync();
                var dto = movimentations.Select(m => new MovimentHistoryDto
                {
                    DateTimeMoviment = m.DateTimeMoviment,
                    Amount = m.Amount,
                    MovimentTypeEnum = m.MovimentTypeEnum.ToString(),
                    AcountNumber = m.accountNumber
                }).ToList();

                var transfer = await _context.Transfers
                 .Where(t => t.AccountNumberFrom == accountNumber || t.AccountNumberTo == accountNumber)
                 .OrderByDescending(t => t.TransferDate)
                 .ToListAsync();

                var transferDtos = transfer.Select(t => new MovimentHistoryDto
                {
                    AcountNumberTo = t.AccountNumberTo,
                    Amount = t.Amount,
                    MovimentTypeEnum = "Transfer",
                    DateTimeMoviment = t.TransferDate,
                }).ToList();

                var result = dto.Concat(transferDtos).ToList();

                response.Data = (List<MovimentHistoryDto>?)result;
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Erro ao buscar movimentações: {ex.Message}";
                return response;
            }
        }
    }
}
