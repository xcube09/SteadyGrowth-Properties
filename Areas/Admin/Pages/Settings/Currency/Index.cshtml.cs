using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Settings.Currency
{
    [Authorize(Policy = "AdminPolicy")]
    public class IndexModel : PageModel
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ICurrencyService currencyService, ILogger<IndexModel> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        public IEnumerable<Models.Entities.Currency> Currencies { get; set; } = new List<Models.Entities.Currency>();
        public Models.Entities.Currency? DefaultCurrency { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Currencies = await _currencyService.GetAllCurrenciesAsync();
                DefaultCurrency = await _currencyService.GetDefaultCurrencyAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currencies");
                StatusMessage = "Error loading currencies. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSetDefaultAsync(int id)
        {
            try
            {
                var result = await _currencyService.SetDefaultCurrencyAsync(id);
                if (result)
                {
                    StatusMessage = "Default currency updated successfully.";
                }
                else
                {
                    StatusMessage = "Failed to update default currency.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default currency");
                StatusMessage = "Error setting default currency. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            try
            {
                var currency = await _currencyService.GetCurrencyByIdAsync(id);
                if (currency != null && !currency.IsDefault)
                {
                    currency.IsActive = !currency.IsActive;
                    await _currencyService.UpdateCurrencyAsync(currency);
                    StatusMessage = $"Currency {(currency.IsActive ? "activated" : "deactivated")} successfully.";
                }
                else
                {
                    StatusMessage = "Cannot deactivate the default currency.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling currency status");
                StatusMessage = "Error updating currency status. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var result = await _currencyService.DeleteCurrencyAsync(id);
                if (result)
                {
                    StatusMessage = "Currency deleted successfully.";
                }
                else
                {
                    StatusMessage = "Cannot delete this currency. It may be the default currency or in use.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting currency");
                StatusMessage = "Error deleting currency. Please try again.";
            }

            return RedirectToPage();
        }
    }
}