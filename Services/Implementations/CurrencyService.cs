using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Globalization;

namespace SteadyGrowth.Web.Services.Implementations
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrencyService(
            ApplicationDbContext context,
            ILogger<CurrencyService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync()
        {
            return await _context.Currencies
                .OrderBy(c => c.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
        {
            return await _context.Currencies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Code)
                .ToListAsync();
        }

        public async Task<Currency?> GetCurrencyByCodeAsync(string code)
        {
            return await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == code.ToUpper());
        }

        public async Task<Currency?> GetCurrencyByIdAsync(int id)
        {
            return await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Currency?> GetDefaultCurrencyAsync()
        {
            var defaultCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.IsDefault);

            if (defaultCurrency == null)
            {
                // If no default currency is set, try to get NGN or create it
                defaultCurrency = await GetCurrencyByCodeAsync("NGN");
                if (defaultCurrency == null)
                {
                    await InitializeDefaultCurrenciesAsync();
                    defaultCurrency = await GetCurrencyByCodeAsync("NGN");
                }
            }

            return defaultCurrency;
        }

        public async Task<Currency> CreateCurrencyAsync(Currency currency)
        {
            currency.CreatedAt = DateTime.UtcNow;
            currency.UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();

            // Add initial exchange rate history
            var exchangeRate = new CurrencyExchangeRate
            {
                CurrencyId = currency.Id,
                Rate = currency.ExchangeRate,
                EffectiveDate = DateTime.UtcNow,
                CreatedBy = currency.UpdatedBy,
                Notes = "Initial exchange rate"
            };

            _context.CurrencyExchangeRates.Add(exchangeRate);
            await _context.SaveChangesAsync();

            return currency;
        }

        public async Task<Currency> UpdateCurrencyAsync(Currency currency)
        {
            currency.UpdatedAt = DateTime.UtcNow;
            currency.UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            _context.Currencies.Update(currency);
            await _context.SaveChangesAsync();

            return currency;
        }

        public async Task<bool> DeleteCurrencyAsync(int id)
        {
            var currency = await GetCurrencyByIdAsync(id);
            if (currency == null || currency.IsDefault)
            {
                return false;
            }

            currency.IsActive = false;
            currency.UpdatedAt = DateTime.UtcNow;
            currency.UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            _context.Currencies.Update(currency);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetDefaultCurrencyAsync(int currencyId)
        {
            var currency = await GetCurrencyByIdAsync(currencyId);
            if (currency == null)
            {
                return false;
            }

            // Remove default flag from all currencies
            var allCurrencies = await _context.Currencies.ToListAsync();
            foreach (var curr in allCurrencies)
            {
                curr.IsDefault = false;
            }

            // Set the new default
            currency.IsDefault = true;
            currency.UpdatedAt = DateTime.UtcNow;
            currency.UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            await _context.SaveChangesAsync();

            // Update system settings
            var defaultCurrencySetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "DefaultCurrency");

            if (defaultCurrencySetting == null)
            {
                defaultCurrencySetting = new SystemSettings
                {
                    SettingKey = "DefaultCurrency",
                    SettingValue = currency.Code,
                    Description = "System default currency",
                    DataType = "string",
                    IsSystem = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name
                };
                _context.SystemSettings.Add(defaultCurrencySetting);
            }
            else
            {
                defaultCurrencySetting.SettingValue = currency.Code;
                defaultCurrencySetting.UpdatedAt = DateTime.UtcNow;
                defaultCurrencySetting.UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
            {
                return amount;
            }

            var rate = await GetExchangeRateAsync(fromCurrency, toCurrency);
            return amount * rate;
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
            {
                return 1;
            }

            var fromCurr = await GetCurrencyByCodeAsync(fromCurrency);
            var toCurr = await GetCurrencyByCodeAsync(toCurrency);

            if (fromCurr == null || toCurr == null)
            {
                throw new InvalidOperationException($"Currency not found: {(fromCurr == null ? fromCurrency : toCurrency)}");
            }

            // Get the default currency to use as base
            var defaultCurrency = await GetDefaultCurrencyAsync();
            if (defaultCurrency == null)
            {
                throw new InvalidOperationException("No default currency set");
            }

            // If converting from default currency
            if (fromCurr.Id == defaultCurrency.Id)
            {
                return toCurr.ExchangeRate;
            }

            // If converting to default currency
            if (toCurr.Id == defaultCurrency.Id)
            {
                return 1 / fromCurr.ExchangeRate;
            }

            // Converting between two non-default currencies
            // Convert from source to default, then from default to target
            return toCurr.ExchangeRate / fromCurr.ExchangeRate;
        }

        public async Task<CurrencyExchangeRate> UpdateExchangeRateAsync(int currencyId, decimal newRate, string? notes = null)
        {
            var currency = await GetCurrencyByIdAsync(currencyId);
            if (currency == null)
            {
                throw new InvalidOperationException($"Currency with ID {currencyId} not found");
            }

            // End the current rate
            var currentRate = await _context.CurrencyExchangeRates
                .Where(r => r.CurrencyId == currencyId && r.EndDate == null)
                .FirstOrDefaultAsync();

            if (currentRate != null)
            {
                currentRate.EndDate = DateTime.UtcNow;
            }

            // Create new rate
            var newExchangeRate = new CurrencyExchangeRate
            {
                CurrencyId = currencyId,
                Rate = newRate,
                EffectiveDate = DateTime.UtcNow,
                Notes = notes,
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name
            };

            _context.CurrencyExchangeRates.Add(newExchangeRate);

            // Update the currency's current rate
            currency.ExchangeRate = newRate;
            currency.UpdatedAt = DateTime.UtcNow;
            currency.UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            await _context.SaveChangesAsync();

            return newExchangeRate;
        }

        public async Task<IEnumerable<CurrencyExchangeRate>> GetExchangeRateHistoryAsync(int currencyId, int? limit = null)
        {
            var query = _context.CurrencyExchangeRates
                .Where(r => r.CurrencyId == currencyId)
                .OrderByDescending(r => r.EffectiveDate);

            if (limit.HasValue)
            {
                query = (IOrderedQueryable<CurrencyExchangeRate>)query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<string> GetCurrencySymbolAsync(string? currencyCode = null)
        {
            Currency? currency;
            
            if (!string.IsNullOrEmpty(currencyCode))
            {
                currency = await GetCurrencyByCodeAsync(currencyCode);
            }
            else
            {
                currency = await GetDefaultCurrencyAsync();
            }

            return currency?.Symbol ?? "₦";
        }

        public string FormatAmount(decimal amount, string? currencyCode = null)
        {
            var symbol = GetCurrencySymbolAsync(currencyCode).Result;
            return $"{symbol}{amount:N2}";
        }

        public async Task InitializeDefaultCurrenciesAsync()
        {
            var existingCurrencies = await _context.Currencies.AnyAsync();
            if (existingCurrencies)
            {
                return;
            }

            var currencies = new List<Currency>
            {
                new Currency
                {
                    Code = "NGN",
                    Name = "Nigerian Naira",
                    Symbol = "₦",
                    ExchangeRate = 1.00m,
                    IsDefault = true,
                    IsActive = true,
                    DecimalPlaces = 2
                },
                new Currency
                {
                    Code = "USD",
                    Name = "US Dollar",
                    Symbol = "$",
                    ExchangeRate = 1550.00m, // 1 USD = 1550 NGN (example rate)
                    IsDefault = false,
                    IsActive = true,
                    DecimalPlaces = 2
                },
                new Currency
                {
                    Code = "EUR",
                    Name = "Euro",
                    Symbol = "€",
                    ExchangeRate = 1650.00m, // 1 EUR = 1650 NGN (example rate)
                    IsDefault = false,
                    IsActive = true,
                    DecimalPlaces = 2
                },
                new Currency
                {
                    Code = "GBP",
                    Name = "British Pound",
                    Symbol = "£",
                    ExchangeRate = 1950.00m, // 1 GBP = 1950 NGN (example rate)
                    IsDefault = false,
                    IsActive = true,
                    DecimalPlaces = 2
                }
            };

            _context.Currencies.AddRange(currencies);
            await _context.SaveChangesAsync();

            // Add initial exchange rates
            foreach (var currency in currencies)
            {
                var exchangeRate = new CurrencyExchangeRate
                {
                    CurrencyId = currency.Id,
                    Rate = currency.ExchangeRate,
                    EffectiveDate = DateTime.UtcNow,
                    Notes = "Initial setup"
                };
                _context.CurrencyExchangeRates.Add(exchangeRate);
            }

            // Add default currency setting
            var defaultCurrencySetting = new SystemSettings
            {
                SettingKey = "DefaultCurrency",
                SettingValue = "NGN",
                Description = "System default currency",
                DataType = "string",
                IsSystem = true
            };
            _context.SystemSettings.Add(defaultCurrencySetting);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Default currencies initialized successfully");
        }
    }
}