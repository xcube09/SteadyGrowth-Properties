using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Settings.Currency
{
    [Authorize(Policy = "AdminPolicy")]
    public class HistoryModel : PageModel
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<HistoryModel> _logger;

        public HistoryModel(ICurrencyService currencyService, ILogger<HistoryModel> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        public Models.Entities.Currency? Currency { get; set; }
        public IEnumerable<CurrencyExchangeRate> ExchangeRates { get; set; } = new List<CurrencyExchangeRate>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Currency = await _currencyService.GetCurrencyByIdAsync(id.Value);
            if (Currency == null)
            {
                return NotFound();
            }

            ExchangeRates = await _currencyService.GetExchangeRateHistoryAsync(id.Value, 50);
            return Page();
        }
    }
}