using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Application.Queries.Properties;

namespace SteadyGrowth.Web.Application.Queries.Academy
{
    public class GetAllCoursesQuery : IRequest<PaginatedList<Course>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int? PackageFilter { get; set; }

        public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, PaginatedList<Course>>
        {
            private readonly ApplicationDbContext _context;

            public GetAllCoursesQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<PaginatedList<Course>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Courses
                    .Include(c => c.AcademyPackage)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(c => c.Title.Contains(request.SearchTerm) ||
                                             c.Description.Contains(request.SearchTerm));
                }

                if (request.PackageFilter.HasValue)
                {
                    query = query.Where(c => c.AcademyPackageId == request.PackageFilter.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var courses = await query.Skip((request.PageIndex - 1) * request.PageSize)
                                     .Take(request.PageSize)
                                     .ToListAsync(cancellationToken);

                return new PaginatedList<Course>(courses, totalCount, request.PageIndex, request.PageSize);
            }
        }
    }
}
