using Azure.Core;
using Bank.Api.CamposEnum;
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

        public async Task<ResponseModel<List<MovimentHistoryDto>>> GetMovimentations1WeakAsync()
        {
            var response = new ResponseModel<List<MovimentHistoryDto>>();
            try
            {
                var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;
                var date = DateTime.Now.AddDays(-7);

                var movimentations = await _context.Moviments
                 .Where(m => m.accountNumber == accountNumber && m.DateTimeMoviment > date)
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
                 .Where(t => t.AccountNumberFrom == accountNumber || t.AccountNumberTo == accountNumber && t.TransferDate > date)
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

        public async Task<ResponseModel<List<MovimentHistoryDto>>> GetMovimentations1MounthAsync()
        {
            var response = new ResponseModel<List<MovimentHistoryDto>>();
            try
            {
                var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;
                var date = DateTime.Now.AddDays(-30);

                var movimentations = await _context.Moviments
                 .Where(m => m.accountNumber == accountNumber && m.DateTimeMoviment > date)
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
                 .Where(t => t.AccountNumberFrom == accountNumber || t.AccountNumberTo == accountNumber && t.TransferDate > date)
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

        public async Task<ResponseModel<PagesOfMovimentHistoryDto<MovimentHistoryDto>>> GetPagedMovimentationsAsync(MovimentRequestDto pagination)
        {
            var response = new ResponseModel<PagesOfMovimentHistoryDto<MovimentHistoryDto>>();
            try
            {
                var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;

                var movimentationsQuery = _context.Moviments
                   .Where(m => m.accountNumber == accountNumber);

                var transfersQuery = _context.Transfers
                   .Where(t => t.AccountNumberFrom == accountNumber || t.AccountNumberTo == accountNumber);

                var movimentDtos = await movimentationsQuery
                    .Select(m => new MovimentHistoryDto
                    {
                        DateTimeMoviment = m.DateTimeMoviment,
                        Amount = m.Amount,
                        MovimentTypeEnum = m.MovimentTypeEnum.ToString(),
                        AcountNumber = m.accountNumber,
                        AcountNumberTo = null
                    }).ToListAsync();

                var transferDtos = await transfersQuery
                    .Select(t => new MovimentHistoryDto
                    {
                        AcountNumberTo = t.AccountNumberTo,
                        Amount = t.Amount,
                        MovimentTypeEnum = "Transfer",
                        DateTimeMoviment = t.TransferDate,
                        AcountNumber = t.AccountNumberFrom
                    }).ToListAsync();

                var allMoviments = movimentDtos
                    .Concat(transferDtos)
                    .OrderByDescending(m => m.DateTimeMoviment)
                    .ToList();

                var totalCount = allMoviments.Count;

                
                var pagedMoviments = allMoviments // Aplica a paginação
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToList();

                
                var pagedResult = new PagesOfMovimentHistoryDto<MovimentHistoryDto>
                {
                    Pages = pagedMoviments, // Se Pages for IEnumerable<T>
                    TotalCount = totalCount,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    // PageCout é calculado automaticamente pela propriedade getter
                };

                response.Data = pagedResult;
                response.Message = "Movimentações paginadas obtidas com sucesso.";
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Erro ao buscar movimentações paginadas: {ex.Message}";
                return response;
            }

        }
    }
}
