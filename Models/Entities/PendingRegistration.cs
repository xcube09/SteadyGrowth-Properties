using System;
using System.ComponentModel.DataAnnotations;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Temporarily holds user registrations until payment is confirmed.
    /// Once approved, a real User account is created and this record is soft deleted.
    /// </summary>
    public class PendingRegistration
    {
        public int Id { get; set; }

        // User data
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = null!;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Hashed password stored for later user creation
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = null!;

        // Package & Referral
        [Required]
        public int SelectedPackageId { get; set; }

        /// <summary>
        /// Referral code used during registration (if any)
        /// </summary>
        [StringLength(8)]
        public string? ReferralCode { get; set; }

        // Status tracking
        public PendingRegistrationStatus Status { get; set; } = PendingRegistrationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Admin user who processed (approved/rejected) this registration
        /// </summary>
        [StringLength(450)]
        public string? ProcessedByUserId { get; set; }

        /// <summary>
        /// Admin notes or rejection reason
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// The created User's ID after approval (for reference)
        /// </summary>
        [StringLength(450)]
        public string? CreatedUserId { get; set; }

        // Navigation properties
        public virtual AcademyPackage SelectedPackage { get; set; } = null!;
        public virtual User? ProcessedByUser { get; set; }
        public virtual User? CreatedUser { get; set; }
    }
}
