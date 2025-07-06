using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace SteadyGrowth.Web.Application.Commands.Academy
{
    public class CreateCourseCommand : IRequest<Course>
    {
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

    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Course>
    {
        private readonly ApplicationDbContext _context;

        public CreateCourseCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Course> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            var course = new Course
            {
                Title = request.Title,
                Description = request.Description,
                Content = request.Content,
                VideoUrl = request.VideoUrl,
                Duration = request.Duration,
                Order = request.Order,
                IsActive = request.IsActive,
                AcademyPackageId = request.AcademyPackageId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync(cancellationToken);

            return course;
        }
    }
}
