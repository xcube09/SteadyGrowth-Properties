using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Represents a reward earned by a user.
    /// </summary>
    public class Reward
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public int Points { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MoneyValue { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public RewardType RewardType { get; set; }

        [Required]
        public DateTime EarnedAt { get; set; }

        public bool IsRedeemed { get; set; }

        public DateTime? RedeemedAt { get; set; }

        /// <summary>
        /// Navigation property for the user who earned the reward.
        /// </summary>
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Types of rewards that can be earned.
    /// </summary>
    public enum RewardType
    {
        Registration = 0,
        Referral = 1,
        PropertyApproval = 2,
        Bonus = 3
    }
}
