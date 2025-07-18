using System;
using Azure;
using Microsoft.EntityFrameworkCore;
using Pix.Api.Data;
using Pix.Api.DTOS;
using Pix.Api.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pix.Api.Services.PixService
{
    public class PixService : IPixService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PixService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ResponseModel<string>> RegistroPix(RegistroPixDto data)
        {
            var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;
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
                    accountNumber);
                response.Data = "Chave criada com sucesso";
                return response;
            }
            catch (Exception e)
            {
                response.Message = $"Erro ao realizar registro: {e.Message}";
                return response;
            }

        }

        public async Task<ResponseModel<string>> RegistroTransferencia(TransferDto data)
        {
            var response = new ResponseModel<string>();
            try
            {
                bool existsGoing = await _context.Pix.AnyAsync(p => p.PixKey == data.Going);
                bool existsComing = await _context.Pix.AnyAsync(p => p.PixKey == data.Coming);
                if (existsGoing && existsComing)
                {
                    var id = Guid.NewGuid();
                    var sql = "EXEC RegistrarTrasnferencia @p0, @p1, @p2, @p3";
                    await _context.Database.ExecuteSqlRawAsync(sql,
                        id,
                        data.Going,
                        data.Coming,
                        data.Amount);
                    response.Data = "Transferencia realizada com sucesso";
                    return response;
                }
                else
                {
                    response.Message = "Error";
                    return response;
                }

            }
            catch (Exception e)
            {
                response.Message = $"Erro ao transferir: {e.Message}";
                return response;
            }
        }

        public async Task<ResponseModel<string>> GetNumberAccountAsync(string chave)
        {
            var response = new ResponseModel<string>();

            try
            {
                var number = await _context.Pix.Where(p => p.PixKey == chave).Select(p => p.AccountNumber).FirstOrDefaultAsync();

                response.Data = number;

                return response;
            }
            catch (Exception e)
            {
                response.Message = $"Error: {e.Message}";
                return response;
            }
        }

        public async Task<ResponseModel<bool>> HasPix()
        {
            var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;
            var response = new ResponseModel<bool>();

            bool has = await _context.Pix.AnyAsync(p => p.AccountNumber == accountNumber);
            if (has == true)
            {
                response.Data = true;
                return response;
            }
            else
            {
                response.Message = "Error";
                return response;
            }
        }

        public async Task<ResponseModel<string>> getPixKey()
        {
            var accountNumber = _httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == "AccountNumber")?.Value;
            var response = new ResponseModel<string>();
            var pixAcount = await _context.Pix.Where(p => p.AccountNumber == accountNumber).Select(p => p.PixKey).FirstOrDefaultAsync();
            response.Data = pixAcount;
            return response;
        }
    }
}
