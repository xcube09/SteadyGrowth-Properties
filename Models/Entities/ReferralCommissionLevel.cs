using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Stores configurable commission rates per referral level and package type.
    /// Level 1 = Direct referrer, Level 2 = Indirect referrer (referrer's referrer)
    /// </summary>
    public class ReferralCommissionLevel
    {
        public int Id { get; set; }

        /// <summary>
        /// Referral level: 1 = Direct referrer, 2 = Indirect referrer
        /// </summary>
        [Required]
        public int Level { get; set; }

        /// <summary>
        /// The package this commission rate applies to
        /// </summary>
        [Required]
        public int AcademyPackageId { get; set; }

        /// <summary>
        /// Commission amount in USD
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }

        /// <summary>
        /// Whether this commission level is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual AcademyPackage AcademyPackage { get; set; } = null!;
    }
}
