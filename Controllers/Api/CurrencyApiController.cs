using Microsoft.AspNetCore.Mvc;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyApiController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyApiController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet("convert")]
        public async Task<IActionResult> ConvertCurrency([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount)
        {
            try
            {
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || amount <= 0)
                {
                    return BadRequest(new { error = "Invalid parameters. Please provide valid 'from', 'to' currencies and a positive amount." });
                }

                var convertedAmount = await _currencyService.ConvertAmountAsync(amount, from.ToUpper(), to.ToUpper());
                
                return Ok(new
                {
                    from = from.ToUpper(),
                    to = to.ToUpper(),
                    originalAmount = amount,
                    convertedAmount = convertedAmount,
                    formattedAmount = $"{(to.ToUpper() == "NGN" ? "₦" : "$")}{convertedAmount:N2}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("rate")]
        public async Task<IActionResult> GetExchangeRate([FromQuery] string from, [FromQuery] string to)
        {
            try
            {
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                {
                    return BadRequest(new { error = "Invalid parameters. Please provide valid 'from' and 'to' currencies." });
                }

                var rate = await _currencyService.GetExchangeRateAsync(from.ToUpper(), to.ToUpper());
                
                return Ok(new
                {
                    from = from.ToUpper(),
                    to = to.ToUpper(),
                    rate = rate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}