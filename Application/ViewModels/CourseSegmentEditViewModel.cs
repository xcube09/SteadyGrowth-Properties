using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Application.ViewModels
{
    public class CourseSegmentEditViewModel
    {
        public int Id { get; set; } // 0 for new segments, > 0 for existing
        
        [Required(ErrorMessage = "Segment title is required.")]
        [StringLength(300, ErrorMessage = "Segment title cannot exceed 300 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Segment content is required.")]
        public string Content { get; set; } = string.Empty;

        public string? VideoUrl { get; set; }

        [Required(ErrorMessage = "Order is required.")]
        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false; // For marking segments for deletion
    }
}