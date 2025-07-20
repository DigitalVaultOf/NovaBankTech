using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Api.Data;
using Payments.Api.DTOS;
using Payments.Api.Services.PaymentService;

namespace Payments.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService, AppDbContext context) : ControllerBase
{
    #region v2_endpoints

    /// <summary>
    /// Gera um novo boleto para o usuário autenticado
    /// </summary>
    [HttpPost("GenerateBankSlip")]
    public async Task<IActionResult> GenerateBankSlipAsync([FromBody] GenerateBankSlipDto dto)
    {
        var response = await paymentService.GenerateBankSlipAsync(dto);
        return !string.IsNullOrEmpty(response.Data)
            ? Ok(response)
            : BadRequest(response);
    }

    /// <summary>
    /// Valida um boleto pelo número e retorna seus dados
    /// </summary>
    [HttpGet("ValidateBankSlip/{bankSlipNumber}")]
    public async Task<IActionResult> ValidateBankSlipAsync(string bankSlipNumber)
    {
        var response = await paymentService.ValidateBankSlipAsync(bankSlipNumber);
        return response.Data is not null ? Ok(response) : NotFound(response);
    }

    /// <summary>
    /// Lista todos os boletos pendentes do usuário autenticado
    /// </summary>
    [HttpGet("GetPendingBankSlips")]
    public async Task<IActionResult> GetPendingBankSlipsAsync()
    {
        var response = await paymentService.GetPendingBankSlipsAsync();
        return Ok(response);
    }

    /// <summary>
    /// Lista todos os boletos pagos do usuário autenticado
    /// </summary>
    [HttpGet("GetPaidBankSlips")]
    public async Task<IActionResult> GetPaidBankSlipsAsync()
    {
        var response = await paymentService.GetPaidBankSlipsAsync();
        return Ok(response);
    }

    /// <summary>
    /// Processa o pagamento de um boleto usando a procedure (V1 - Pagamento integral)
    /// </summary>
    [HttpPost("PayBankSlip")]
    public async Task<IActionResult> PayBankSlipAsync([FromBody] PayBankSlipDto dto)
    {
        var response = await paymentService.PayBankSlipAsync(dto);

        // ✅ CORRIGIDO: PayBankSlipAsync retorna ResponseModel<bool>
        // response.Data é bool (true/false para sucesso/erro)
        return response.Data ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Processa o pagamento parcial de um boleto usando a procedure (V2 - Pagamento parcial/integral)
    /// </summary>
    [HttpPost("PayPartialBankSlip")]
    public async Task<IActionResult> PayPartialBankSlip([FromBody] PayPartialBankSlipDto dto)
    {
        var response = await paymentService.PayPartialBankSlipAsync(dto);

        // ✅ CORRIGIDO: PayPartialBankSlipAsync retorna ResponseModel<PayBankSlipResponseDto>
        // response.Data é PayBankSlipResponseDto que tem propriedade IsSuccess
        return response.Data?.IsSuccess == true ? Ok(response) : BadRequest(response);
    }

    #endregion

    #region debug_endpoints

    /// <summary>
    /// [DEBUG] Testa se o JWT está sendo processado corretamente
    /// </summary>
    [Authorize]
    [HttpGet("debug-jwt")]
    public IActionResult DebugJwt()
    {
        try
        {
            var user = HttpContext.User;

            var result = new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
                AuthenticationType = user.Identity?.AuthenticationType,
                Claims = user.Claims.Select(c => new
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList(),
                Headers = Request.Headers.Where(h => h.Key.ToLower().Contains("auth"))
                    .ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            Console.WriteLine($"DEBUG JWT: IsAuthenticated = {result.IsAuthenticated}");
            Console.WriteLine($"DEBUG JWT: Claims count = {result.Claims.Count}");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// [DEBUG] Lista TODOS os boletos no banco para debug
    /// </summary>
    [HttpGet("debug-all-boletos")]
    public async Task<IActionResult> DebugAllBoletosAsync()
    {
        try
        {
            var allBoletos = await context.MonthPayments.ToListAsync();

            var result = allBoletos.Select(b => new
            {
                PaymentId = b.PaymentId,
                BankSlipNumber = b.BankSlipNumber,
                Amount = b.Amount,
                AccountNumber = b.AccountNumber,
                UserId = b.UserId,
                IsPaid = b.IsPaid,
                CreatedAt = b.CreatedAt,
                Description = b.Description
            }).ToList();

            Console.WriteLine($"DEBUG ALL: Found {result.Count} boletos in database");
            foreach (var boleto in result)
            {
                Console.WriteLine(
                    $"DEBUG ALL: BankSlipNumber={boleto.BankSlipNumber}, Amount={boleto.Amount}, IsPaid={boleto.IsPaid}");
            }

            return Ok(new
            {
                count = result.Count,
                boletos = result,
                message = $"Total de {result.Count} boletos encontrados"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    #endregion

    #region v1_endpoints

    [HttpGet("GetMonthPayment")]
    public async Task<IActionResult> GetMonthPaymentAsync([FromQuery] MonthPaymentDto dto)
    {
        var response = await paymentService.GetMonthPaymentsAsync(dto);
        return response.Data is not null ? Ok(response) : NotFound(response);
    }

    [HttpPost("AddPayment")]
    public async Task<IActionResult> AddPaymentAsync([FromBody] AddPaymentDto dto)
    {
        var response = await paymentService.AddPaymentAsync(dto);
        return response.Data ? Ok(response) : BadRequest(response);
    }

    [HttpPost("MarkAsPaid")]
    public async Task<IActionResult> MarkAsPaidAsync([FromBody] PayBankSlipDto dto)
    {
        var response = await paymentService.MarkAsPaidAsync(dto);
        return response.Data ? Ok(response) : BadRequest(response);
    }

    [HttpGet("GetPaymentHistory/{userId:guid}")]
    public async Task<IActionResult> GetPaymentHistoryAsync(Guid userId)
    {
        var response = await paymentService.GetPaymentsHistoryAsync(userId);
        return Ok(response);
    }

    [HttpPost("GenerateMonthlyPayment/{userId:guid}")]
    public async Task<IActionResult> GenerateMonthlyPaymentAsync(Guid userId)
    {
        var response = await paymentService.GenerateMonthlyPaymentAsync(userId);
        return response.Data ? Ok(response) : BadRequest(response);
    }

    #endregion
}