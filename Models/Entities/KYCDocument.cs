using SteadyGrowth.Web.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    public class KYCDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public DocumentType DocumentType { get; set; }

        [Required]
        [StringLength(500)]
        public string FileName { get; set; } = string.Empty; // Relative path to the stored file

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }
}
