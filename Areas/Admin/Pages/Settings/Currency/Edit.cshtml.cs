using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Settings.Currency
{
    [Authorize(Policy = "AdminPolicy")]
    public class EditModel : PageModel
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ICurrencyService currencyService, ILogger<EditModel> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public Models.Entities.Currency? Currency { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public decimal OriginalExchangeRate { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [StringLength(3, MinimumLength = 3)]
            [Display(Name = "Currency Code")]
            public string Code { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            [Display(Name = "Currency Name")]
            public string Name { get; set; } = string.Empty;

            [Required]
            [StringLength(10)]
            [Display(Name = "Symbol")]
            public string Symbol { get; set; } = string.Empty;

            [Required]
            [Range(0.000001, double.MaxValue, ErrorMessage = "Exchange rate must be greater than 0")]
            [Display(Name = "Exchange Rate")]
            public decimal ExchangeRate { get; set; } = 1.00m;

            [Required]
            [Range(0, 4)]
            [Display(Name = "Decimal Places")]
            public int DecimalPlaces { get; set; } = 2;

            [Display(Name = "Set as Default")]
            public bool IsDefault { get; set; }

            [Display(Name = "Active")]
            public bool IsActive { get; set; } = true;

            [StringLength(500)]
            [Display(Name = "Notes for Rate Change")]
            public string? RateChangeNotes { get; set; }

            [Display(Name = "Force Rate Update")]
            public bool ForceRateUpdate { get; set; }
        }

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

            OriginalExchangeRate = Currency.ExchangeRate;

            Input = new InputModel
            {
                Id = Currency.Id,
                Code = Currency.Code,
                Name = Currency.Name,
                Symbol = Currency.Symbol,
                ExchangeRate = Currency.ExchangeRate,
                DecimalPlaces = Currency.DecimalPlaces,
                IsDefault = Currency.IsDefault,
                IsActive = Currency.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload currency data for the view
                Currency = await _currencyService.GetCurrencyByIdAsync(Input.Id);
                if (Currency != null)
                {
                    OriginalExchangeRate = Currency.ExchangeRate;
                }
                return Page();
            }

            try
            {
                var currency = await _currencyService.GetCurrencyByIdAsync(Input.Id);
                if (currency == null)
                {
                    return NotFound();
                }

                OriginalExchangeRate = currency.ExchangeRate;

                // Check if exchange rate has changed significantly
                bool rateChanged = Math.Abs(currency.ExchangeRate - Input.ExchangeRate) > 0.000001m;

                // If rate changed but no notes provided and not forced, require notes
                if (rateChanged && string.IsNullOrEmpty(Input.RateChangeNotes) && !Input.ForceRateUpdate)
                {
                    ModelState.AddModelError("Input.RateChangeNotes", "Please provide notes explaining the exchange rate change, or check 'Force Rate Update' to proceed without notes.");
                    Currency = currency;
                    return Page();
                }

                // Validate that we're not trying to deactivate the default currency
                if (currency.IsDefault && !Input.IsActive)
                {
                    ModelState.AddModelError("Input.IsActive", "Cannot deactivate the default currency. Please set another currency as default first.");
                    Currency = currency;
                    return Page();
                }

                // Update currency properties
                currency.Name = Input.Name;
                currency.Symbol = Input.Symbol;
                currency.DecimalPlaces = Input.DecimalPlaces;
                
                // Only update IsActive if it's not the default currency or if we're not trying to deactivate it
                if (!currency.IsDefault || Input.IsActive)
                {
                    currency.IsActive = Input.IsActive;
                }

                // If rate changed, update through the service to maintain history
                if (rateChanged)
                {
                    await _currencyService.UpdateExchangeRateAsync(currency.Id, Input.ExchangeRate, 
                        string.IsNullOrEmpty(Input.RateChangeNotes) ? "Rate updated without notes" : Input.RateChangeNotes);
                    StatusMessage = $"Currency updated successfully. Exchange rate changed from {OriginalExchangeRate:N6} to {Input.ExchangeRate:N6}.";
                }
                else
                {
                    await _currencyService.UpdateCurrencyAsync(currency);
                    StatusMessage = "Currency updated successfully.";
                }

                // Handle default currency setting
                if (Input.IsDefault && !currency.IsDefault)
                {
                    var result = await _currencyService.SetDefaultCurrencyAsync(currency.Id);
                    if (result)
                    {
                        StatusMessage += " This currency has been set as the new default.";
                    }
                    else
                    {
                        StatusMessage += " However, there was an issue setting it as default.";
                    }
                }

                TempData["StatusMessage"] = StatusMessage;
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating currency");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the currency. Please try again.");
                
                // Reload currency data for the view
                Currency = await _currencyService.GetCurrencyByIdAsync(Input.Id);
                if (Currency != null)
                {
                    OriginalExchangeRate = Currency.ExchangeRate;
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostQuickRateUpdateAsync(int id, decimal newRate, string? notes)
        {
            try
            {
                await _currencyService.UpdateExchangeRateAsync(id, newRate, notes ?? "Quick rate update");
                TempData["StatusMessage"] = "Exchange rate updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rate");
                TempData["StatusMessage"] = "Error updating exchange rate. Please try again.";
            }

            return RedirectToPage(new { id });
        }
    }
}