using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    public enum UpgradeRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public class UpgradeRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int RequestedPackageId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // e.g., "Bank Transfer"

        [StringLength(500)]
        public string? PaymentDetails { get; set; } // e.g., transaction ID, bank account info

        public UpgradeRequestStatus Status { get; set; } = UpgradeRequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(RequestedPackageId))]
        public virtual AcademyPackage RequestedPackage { get; set; } = null!;
    }
}
