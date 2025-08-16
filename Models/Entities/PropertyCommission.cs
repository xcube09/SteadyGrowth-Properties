using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Tracks commissions earned by users on property sales
    /// </summary>
    public class PropertyCommission
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public int PropertyId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }
        
        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Reference { get; set; }
        
        [Required]
        public string AddedByUserId { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Reference to the wallet transaction created for this commission
        /// </summary>
        public int? WalletTransactionId { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey(nameof(PropertyId))]
        public virtual Property Property { get; set; } = null!;
        
        [ForeignKey(nameof(AddedByUserId))]
        public virtual User AddedBy { get; set; } = null!;
        
        [ForeignKey(nameof(WalletTransactionId))]
        public virtual WalletTransaction? WalletTransaction { get; set; }
    }
}