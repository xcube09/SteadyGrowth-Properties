using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Application.Queries.Academy
{
    public class GetUserCourseProgressQueryHandler : IRequestHandler<GetUserCourseProgressQuery, List<CourseProgress>>
    {
        private readonly ApplicationDbContext _context;

        public GetUserCourseProgressQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseProgress>> Handle(GetUserCourseProgressQuery request, CancellationToken cancellationToken)
        {
            return await _context.CourseProgresses
                .Where(cp => cp.UserId == request.UserId)
                .Include(cp => cp.Course) // Include course details if needed
                .ToListAsync(cancellationToken);
        }
    }
}
