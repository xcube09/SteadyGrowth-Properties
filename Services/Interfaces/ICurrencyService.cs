using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces
{
    public interface ICurrencyService
    {
        Task<IEnumerable<Currency>> GetAllCurrenciesAsync();
        Task<IEnumerable<Currency>> GetActiveCurrenciesAsync();
        Task<Currency?> GetCurrencyByCodeAsync(string code);
        Task<Currency?> GetCurrencyByIdAsync(int id);
        Task<Currency?> GetDefaultCurrencyAsync();
        Task<Currency> CreateCurrencyAsync(Currency currency);
        Task<Currency> UpdateCurrencyAsync(Currency currency);
        Task<bool> DeleteCurrencyAsync(int id);
        Task<bool> SetDefaultCurrencyAsync(int currencyId);
        Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency);
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
        Task<CurrencyExchangeRate> UpdateExchangeRateAsync(int currencyId, decimal newRate, string? notes = null);
        Task<IEnumerable<CurrencyExchangeRate>> GetExchangeRateHistoryAsync(int currencyId, int? limit = null);
        Task<string> GetCurrencySymbolAsync(string? currencyCode = null);
        string FormatAmount(decimal amount, string? currencyCode = null);
        Task InitializeDefaultCurrenciesAsync();
    }
}