using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SteadyGrowth.Web.Application.Queries.Academy
{
    public class GetCourseWithSegmentsQuery : IRequest<CourseWithSegments?>
    {
        public int CourseId { get; set; }
    }

    public class CourseWithSegments
    {
        public Course Course { get; set; } = null!;
        public List<CourseSegment> Segments { get; set; } = new();
    }

    public class GetCourseWithSegmentsQueryHandler : IRequestHandler<GetCourseWithSegmentsQuery, CourseWithSegments?>
    {
        private readonly ApplicationDbContext _context;

        public GetCourseWithSegmentsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CourseWithSegments?> Handle(GetCourseWithSegmentsQuery request, CancellationToken cancellationToken)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

            if (course == null)
            {
                return null;
            }

            var segments = await _context.CourseSegments
                .Where(cs => cs.CourseId == request.CourseId)
                .OrderBy(cs => cs.Order)
                .ToListAsync(cancellationToken);

            return new CourseWithSegments
            {
                Course = course,
                Segments = segments
            };
        }
    }
}