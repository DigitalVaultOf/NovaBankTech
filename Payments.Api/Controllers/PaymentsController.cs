using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Api.DTOS;
using Payments.Api.Services.PaymentService;

namespace Payments.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
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
    public async Task<IActionResult> MarkAsPaidAsync([FromBody] PaySlipDto dto)
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
}