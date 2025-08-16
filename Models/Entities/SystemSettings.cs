using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// System settings entity for storing global configuration
    /// </summary>
    public class SystemSettings
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        public string SettingValue { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? DataType { get; set; } = "string";

        public bool IsSystem { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }
    }
}