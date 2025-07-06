using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// User entity extending IdentityUser for authentication
    /// </summary>
    public class User : IdentityUser
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(8)]
        public string? ReferredBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(8)]
        public string? ReferralCode { get; set; }

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public int? AcademyPackageId { get; set; }
        public virtual AcademyPackage? AcademyPackage { get; set; }

        // Navigation properties
        public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
        public virtual ICollection<Referral> ReferralsMade { get; set; } = new List<Referral>();
        public virtual ICollection<Referral> ReferralsReceived { get; set; } = new List<Referral>();
    }
} 