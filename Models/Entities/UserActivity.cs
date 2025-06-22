using System;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Represents a user activity event in the system.
    /// </summary>
    public class UserActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public ActivityType ActivityType { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation property for the user who performed the activity.
        /// </summary>
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Types of user activities tracked in the system.
    /// </summary>
    public enum ActivityType
    {
        Login = 0,
        PropertyCreated = 1,
        PropertyApproved = 2,
        ReferralMade = 3,
        RewardEarned = 4
    }
}
