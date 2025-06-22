using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Represents a log entry for property vetting actions.
    /// </summary>
    public class VettingLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public string AdminUserId { get; set; } = null!;

        [Required]
        public VettingAction Action { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation property for the property being vetted.
        /// </summary>
        public virtual Property Property { get; set; } = null!;

        /// <summary>
        /// Navigation property for the admin user who performed the action.
        /// </summary>
        public virtual User AdminUser { get; set; } = null!;
    }

    /// <summary>
    /// Actions that can be taken during property vetting.
    /// </summary>
    public enum VettingAction
    {
        Submitted = 0,
        UnderReview = 1,
        Approved = 2,
        Rejected = 3,
        RequestChanges = 4
    }
}
