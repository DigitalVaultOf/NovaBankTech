using Microsoft.EntityFrameworkCore;
using Pix.Api.Data;
using Pix.Api.DTOS;
using Pix.Api.Models;

namespace Pix.Api.Services.PixService
{
    public class PixService : IPixService
    {
        private readonly AppDbContext _context;

        public PixService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ResponseModel<string>> RegistroPix(RegistroPixDto data)
        {
            var response = new ResponseModel<string>();
            try
            {
                var id = Guid.NewGuid();
                var sql = "EXEC CriarChave @p0, @p1, @p2, @p3, @p4";
                await _context.Database.ExecuteSqlRawAsync(sql,
                    id,
                    data.Name,
                    data.PixKey,
                    data.Bank,
                    data.Account);
                response.Data = "Chave criada com sucesso";
                return response;
            }
            catch(Exception e) {
                response.Message = $"Erro ao realizar registro: {e.Message}";
                return response;
            }

        }
    }
}
