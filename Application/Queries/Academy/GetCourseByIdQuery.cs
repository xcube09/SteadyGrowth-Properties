using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Queries.Academy
{
    public class GetCourseByIdQuery : IRequest<Course>
    {
        public int Id { get; set; }

        public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, Course>
        {
            private readonly ApplicationDbContext _context;

            public GetCourseByIdQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Course> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
            {
                return await _context.Courses.FindAsync(new object[] { request.Id }, cancellationToken);
            }
        }
    }
}
