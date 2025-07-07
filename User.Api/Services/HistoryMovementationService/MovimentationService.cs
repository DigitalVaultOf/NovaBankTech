using Bank.Api.DTOS;
using Bank.Api.Repositories.Interfaces;
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

                var dto = new List<MovimentHistoryDto>();
                response.Data = dto;
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
