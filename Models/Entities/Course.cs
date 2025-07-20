using System;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Models.Entities
{
    /// <summary>
    /// Represents an educational course.
    /// </summary>
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? VideoUrl { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public int Order { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public int TotalLessons { get; set; } = 0;

        public int? AcademyPackageId { get; set; }
        public virtual AcademyPackage? AcademyPackage { get; set; }
    }
}
