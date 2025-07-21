using SteadyGrowth.Web.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    public enum WithdrawalStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class WithdrawalRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

        [Required]
        [StringLength(200)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string AccountName { get; set; } = string.Empty;

        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedDate { get; set; }

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }
}
