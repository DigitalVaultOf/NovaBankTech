using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Api.DTOS;
using Payments.Api.Services.PaymentService;

namespace Payments.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(IPaymentService paymentService) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;

        [HttpGet("GetMonthPayment")]
        public async Task<IActionResult> GetMonthPaymentAsync([FromQuery] MonthPaymentDto dto)
        {
            var response = await _paymentService.GetMonthPaymentAsync(dto);
            return response.Data is not null ? Ok(response) : NotFound(response);
        }
    }
}