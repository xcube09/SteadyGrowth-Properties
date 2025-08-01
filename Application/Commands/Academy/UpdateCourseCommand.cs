using MediatR;
using SteadyGrowth.Web.Data;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Commands.Academy
{
    public class UpdateCourseCommand : IRequest<bool>
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(300, ErrorMessage = "Title cannot exceed 300 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course content is required.")]
        public string Content { get; set; } = string.Empty;

        public string? VideoUrl { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be a positive value.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Order is required.")]
        public int Order { get; set; }

        public bool IsActive { get; set; }

        public int? AcademyPackageId { get; set; }
    }

    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public UpdateCourseCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            var course = await _context.Courses.FindAsync(new object[] { request.Id }, cancellationToken);

            if (course == null)
            {
                return false;
            }

            course.Title = request.Title;
            course.Description = request.Description;
            course.Content = request.Content;
            course.VideoUrl = request.VideoUrl;
            course.Duration = request.Duration;
            course.Order = request.Order;
            course.IsActive = request.IsActive;
            course.AcademyPackageId = request.AcademyPackageId;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
