using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    public enum WalletTransactionType
    {
        Credit = 0,
        Debit = 1,
        Refund = 2,
        Bonus = 3,
        Commission = 4
    }

    public enum WalletTransactionStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Cancelled = 3
    }

    /// <summary>
    /// Wallet entity representing a user's wallet
    /// </summary>
    public class Wallet
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        public virtual ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }

    /// <summary>
    /// Wallet transaction entity for tracking all wallet activities
    /// </summary>
    public class WalletTransaction
    {
        public int Id { get; set; }

        [Required]
        public int WalletId { get; set; }

        [Required]
        public WalletTransactionType TransactionType { get; set; }

        [Required]
        public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceBefore { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        [Required, StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reference { get; set; }

        [StringLength(450)]
        public string? AdminUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        // Navigation properties
        [ForeignKey("WalletId")]
        public virtual Wallet Wallet { get; set; } = null!;

        [ForeignKey("AdminUserId")]
        public virtual User? AdminUser { get; set; }
    }
} 