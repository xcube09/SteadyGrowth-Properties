using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Tracks user progress on individual course segments
    /// </summary>
    public class SegmentProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public int CourseSegmentId { get; set; }

        [ForeignKey("CourseSegmentId")]
        public virtual CourseSegment CourseSegment { get; set; } = null!;

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    }
}