using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Represents an image associated with a property listing.
    /// </summary>
    public class PropertyImage
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required, StringLength(300)]
        public string FileName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Caption { get; set; }

        [StringLength(100)]
        public string? ImageType { get; set; } // e.g. "Exterior", "Interior", "Floor Plan", etc.

        public int DisplayOrder { get; set; } = 0;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; } = null!;
    }
}
