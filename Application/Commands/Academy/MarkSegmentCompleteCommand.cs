using MediatR;
using System.Threading;
using System.Threading.Tasks;
using SteadyGrowth.Web.Data;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Models.Entities;
using System;
using System.Linq;

namespace SteadyGrowth.Web.Application.Commands.Academy
{
    public class MarkSegmentCompleteCommand : IRequest<bool>
    {
        public string UserId { get; set; } = string.Empty;
        public int CourseSegmentId { get; set; }
    }

    public class MarkSegmentCompleteCommandHandler : IRequestHandler<MarkSegmentCompleteCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public MarkSegmentCompleteCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(MarkSegmentCompleteCommand request, CancellationToken cancellationToken)
        {
            // Get or create segment progress
            var segmentProgress = await _context.SegmentProgresses
                .FirstOrDefaultAsync(sp => sp.UserId == request.UserId && sp.CourseSegmentId == request.CourseSegmentId, cancellationToken);

            if (segmentProgress == null)
            {
                segmentProgress = new SegmentProgress
                {
                    UserId = request.UserId,
                    CourseSegmentId = request.CourseSegmentId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow
                };
                _context.SegmentProgresses.Add(segmentProgress);
            }
            else if (!segmentProgress.IsCompleted)
            {
                segmentProgress.IsCompleted = true;
                segmentProgress.CompletedAt = DateTime.UtcNow;
                segmentProgress.LastAccessedAt = DateTime.UtcNow;
            }
            else
            {
                // Already completed
                segmentProgress.LastAccessedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Update overall course progress
            await UpdateCourseProgress(request.UserId, request.CourseSegmentId, cancellationToken);

            return true;
        }

        private async Task UpdateCourseProgress(string userId, int courseSegmentId, CancellationToken cancellationToken)
        {
            // Get the course ID from the segment
            var segment = await _context.CourseSegments
                .FirstOrDefaultAsync(cs => cs.Id == courseSegmentId, cancellationToken);

            if (segment == null) return;

            // Get or create course progress
            var courseProgress = await _context.CourseProgresses
                .Include(cp => cp.Course)
                .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.CourseId == segment.CourseId, cancellationToken);

            if (courseProgress == null)
            {
                var course = await _context.Courses.FindAsync(segment.CourseId);
                if (course == null) return;

                courseProgress = new CourseProgress
                {
                    UserId = userId,
                    CourseId = segment.CourseId,
                    CompletedLessonsCount = 0,
                    IsCompleted = false,
                    LastAccessedDate = DateTime.UtcNow
                };
                _context.CourseProgresses.Add(courseProgress);
            }

            // Count completed segments for this course
            var completedSegmentsCount = await _context.SegmentProgresses
                .Join(_context.CourseSegments,
                    sp => sp.CourseSegmentId,
                    cs => cs.Id,
                    (sp, cs) => new { sp, cs })
                .Where(x => x.sp.UserId == userId && x.cs.CourseId == segment.CourseId && x.sp.IsCompleted)
                .CountAsync(cancellationToken);

            // Get total segments for this course
            var totalSegments = await _context.CourseSegments
                .Where(cs => cs.CourseId == segment.CourseId && cs.IsActive)
                .CountAsync(cancellationToken);

            // Update course progress
            courseProgress.CompletedLessonsCount = completedSegmentsCount;
            courseProgress.IsCompleted = completedSegmentsCount >= totalSegments;
            courseProgress.LastAccessedDate = DateTime.UtcNow;

            // Update course's total lessons if needed
            if (courseProgress.Course.TotalLessons != totalSegments)
            {
                courseProgress.Course.TotalLessons = totalSegments;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}