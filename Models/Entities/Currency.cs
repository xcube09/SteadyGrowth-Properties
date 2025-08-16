using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Currency entity for managing different currencies in the system
    /// </summary>
    public class Currency
    {
        public int Id { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Symbol { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1.00m;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int DecimalPlaces { get; set; } = 2;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<CurrencyExchangeRate> ExchangeRates { get; set; } = new List<CurrencyExchangeRate>();
    }

    /// <summary>
    /// Currency exchange rate history for tracking rate changes over time
    /// </summary>
    public class CurrencyExchangeRate
    {
        public int Id { get; set; }

        [Required]
        public int CurrencyId { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal Rate { get; set; }

        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CurrencyId")]
        public virtual Currency Currency { get; set; } = null!;
    }
}