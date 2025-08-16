using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Settings.Currency
{
    [Authorize(Policy = "AdminPolicy")]
    public class AddModel : PageModel
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<AddModel> _logger;

        public AddModel(ICurrencyService currencyService, ILogger<AddModel> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(3, MinimumLength = 3)]
            [Display(Name = "Currency Code")]
            [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency code must be exactly 3 uppercase letters (e.g., USD, EUR)")]
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
            [Display(Name = "Initial Rate Notes")]
            public string? Notes { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check if currency code already exists
                var existingCurrency = await _currencyService.GetCurrencyByCodeAsync(Input.Code);
                if (existingCurrency != null)
                {
                    ModelState.AddModelError("Input.Code", "A currency with this code already exists.");
                    return Page();
                }

                var currency = new Models.Entities.Currency
                {
                    Code = Input.Code.ToUpper(),
                    Name = Input.Name,
                    Symbol = Input.Symbol,
                    ExchangeRate = Input.ExchangeRate,
                    DecimalPlaces = Input.DecimalPlaces,
                    IsDefault = false, // Will be set later if needed
                    IsActive = Input.IsActive
                };

                var createdCurrency = await _currencyService.CreateCurrencyAsync(currency);

                // Add initial exchange rate with notes if provided
                if (!string.IsNullOrEmpty(Input.Notes))
                {
                    await _currencyService.UpdateExchangeRateAsync(createdCurrency.Id, Input.ExchangeRate, Input.Notes);
                }

                if (Input.IsDefault)
                {
                    await _currencyService.SetDefaultCurrencyAsync(createdCurrency.Id);
                }

                TempData["StatusMessage"] = "Currency added successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding currency");
                ModelState.AddModelError(string.Empty, "An error occurred while adding the currency. Please try again.");
                return Page();
            }
        }
    }
}