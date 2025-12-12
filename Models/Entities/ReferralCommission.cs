using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Tracks earned commissions when packages are purchased through referrals.
    /// </summary>
    public class ReferralCommission
    {
        public int Id { get; set; }

        /// <summary>
        /// User receiving the commission (the referrer)
        /// </summary>
        [Required]
        public string BeneficiaryUserId { get; set; } = null!;

        /// <summary>
        /// User who purchased the package (the one who was referred)
        /// </summary>
        [Required]
        public string SourceUserId { get; set; } = null!;

        /// <summary>
        /// The package that was purchased
        /// </summary>
        [Required]
        public int AcademyPackageId { get; set; }

        /// <summary>
        /// Referral level: 1 = Direct referrer, 2 = Indirect referrer
        /// </summary>
        [Required]
        public int Level { get; set; }

        /// <summary>
        /// Commission amount earned in USD
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }

        /// <summary>
        /// Link to the wallet transaction when commission is paid
        /// </summary>
        public int? WalletTransactionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User BeneficiaryUser { get; set; } = null!;
        public virtual User SourceUser { get; set; } = null!;
        public virtual AcademyPackage AcademyPackage { get; set; } = null!;
        public virtual WalletTransaction? WalletTransaction { get; set; }
    }
}
