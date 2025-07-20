using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Application.Queries.Academy
{
    public class GetPaginatedCoursesQueryHandler : IRequestHandler<GetPaginatedCoursesQuery, PaginatedResultDto<Course>>
    {
        private readonly ApplicationDbContext _context;

        public GetPaginatedCoursesQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResultDto<Course>> Handle(GetPaginatedCoursesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Courses.AsQueryable();

            if (!request.IsPremiumMember)
            {
                var basicPackage = await _context.AcademyPackages.FirstOrDefaultAsync(ap => ap.Name == "Basic Package", cancellationToken);
                if (basicPackage != null)
                {
                    query = query.Where(c => c.AcademyPackageId == basicPackage.Id);
                }
                else
                {
                    query = query.Where(c => c.AcademyPackageId == null); // Fallback if basic package not found
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var courses = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResultDto<Course>(courses, totalCount, request.PageIndex, request.PageSize);
        }
    }
}
