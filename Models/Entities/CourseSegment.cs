using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Represents a segment/lesson within a course
    /// </summary>
    public class CourseSegment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? VideoUrl { get; set; }

        [Required]
        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}