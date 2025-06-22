using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Referral tracking entity
    /// </summary>
    public class Referral
    {
        public int Id { get; set; }
        
        [Required]
        public string ReferrerId { get; set; } = string.Empty;
        
        [Required]
        public string ReferredUserId { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionEarned { get; set; }
        
        public bool CommissionPaid { get; set; }
        
        public DateTime? PaidAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        [ForeignKey(nameof(ReferrerId))]
        public virtual User Referrer { get; set; } = null!;
        
        [ForeignKey(nameof(ReferredUserId))]
        public virtual User ReferredUser { get; set; } = null!;
    }
} 