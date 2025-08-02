using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Application.Queries.Academy
{
    public class GetCourseSegmentsQuery : IRequest<List<CourseSegment>>
    {
        public int CourseId { get; set; }
        public string? UserId { get; set; } // Optional: for progress tracking
    }

    public class CourseSegmentWithProgress
    {
        public CourseSegment Segment { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }

    public class GetCourseSegmentsWithProgressQuery : IRequest<List<CourseSegmentWithProgress>>
    {
        public int CourseId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class GetCourseSegmentsQueryHandler : IRequestHandler<GetCourseSegmentsQuery, List<CourseSegment>>
    {
        private readonly ApplicationDbContext _context;

        public GetCourseSegmentsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseSegment>> Handle(GetCourseSegmentsQuery request, CancellationToken cancellationToken)
        {
            return await _context.CourseSegments
                .Where(cs => cs.CourseId == request.CourseId && cs.IsActive)
                .OrderBy(cs => cs.Order)
                .ToListAsync(cancellationToken);
        }
    }

    public class GetCourseSegmentsWithProgressQueryHandler : IRequestHandler<GetCourseSegmentsWithProgressQuery, List<CourseSegmentWithProgress>>
    {
        private readonly ApplicationDbContext _context;

        public GetCourseSegmentsWithProgressQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseSegmentWithProgress>> Handle(GetCourseSegmentsWithProgressQuery request, CancellationToken cancellationToken)
        {
            var segments = await _context.CourseSegments
                .Where(cs => cs.CourseId == request.CourseId && cs.IsActive)
                .OrderBy(cs => cs.Order)
                .ToListAsync(cancellationToken);

            var segmentProgresses = await _context.SegmentProgresses
                .Where(sp => sp.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            var result = segments.Select(segment =>
            {
                var progress = segmentProgresses.FirstOrDefault(sp => sp.CourseSegmentId == segment.Id);
                return new CourseSegmentWithProgress
                {
                    Segment = segment,
                    IsCompleted = progress?.IsCompleted ?? false,
                    CompletedAt = progress?.CompletedAt,
                    LastAccessedAt = progress?.LastAccessedAt
                };
            }).ToList();

            return result;
        }
    }
}