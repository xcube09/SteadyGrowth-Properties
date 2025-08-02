using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Application.ViewModels;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Required(ErrorMessage = "Duration is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be a positive value.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Order is required.")]
        public int Order { get; set; }

        public bool IsActive { get; set; }

        public int? AcademyPackageId { get; set; }

        public List<CourseSegmentEditViewModel> Segments { get; set; } = new();
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

            // Update course properties
            course.Title = request.Title;
            course.Description = request.Description;
            course.Content = string.Empty; // Will be populated by segments
            course.VideoUrl = null; // Will be populated by segments
            course.Duration = request.Duration;
            course.Order = request.Order;
            course.IsActive = request.IsActive;
            course.AcademyPackageId = request.AcademyPackageId;
            course.TotalLessons = request.Segments.Count(s => !s.IsDeleted);
            course.UpdatedAt = DateTime.UtcNow;

            // Handle segments
            await UpdateCourseSegments(request.Id, request.Segments, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task UpdateCourseSegments(int courseId, List<CourseSegmentEditViewModel> segments, CancellationToken cancellationToken)
        {
            // Get existing segments
            var existingSegments = await _context.CourseSegments
                .Where(cs => cs.CourseId == courseId)
                .ToListAsync(cancellationToken);

            // Process segments
            foreach (var segmentVm in segments)
            {
                if (segmentVm.IsDeleted && segmentVm.Id > 0)
                {
                    // Delete existing segment
                    var segmentToDelete = existingSegments.FirstOrDefault(s => s.Id == segmentVm.Id);
                    if (segmentToDelete != null)
                    {
                        _context.CourseSegments.Remove(segmentToDelete);
                    }
                }
                else if (segmentVm.Id > 0)
                {
                    // Update existing segment
                    var existingSegment = existingSegments.FirstOrDefault(s => s.Id == segmentVm.Id);
                    if (existingSegment != null)
                    {
                        existingSegment.Title = segmentVm.Title;
                        existingSegment.Content = segmentVm.Content;
                        existingSegment.VideoUrl = segmentVm.VideoUrl;
                        existingSegment.Order = segmentVm.Order;
                        existingSegment.IsActive = segmentVm.IsActive;
                        existingSegment.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else if (!segmentVm.IsDeleted)
                {
                    // Create new segment
                    var newSegment = new CourseSegment
                    {
                        CourseId = courseId,
                        Title = segmentVm.Title,
                        Content = segmentVm.Content,
                        VideoUrl = segmentVm.VideoUrl,
                        Order = segmentVm.Order,
                        IsActive = segmentVm.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.CourseSegments.Add(newSegment);
                }
            }
        }
    }
}
