using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Application.Commands.Academy
{
    public class DeleteCourseCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }

    public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public DeleteCourseCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _context.Courses.FindAsync(new object[] { request.Id }, cancellationToken);

                if (course == null)
                {
                    return false;
                }

                // First, delete any related CourseProgress records
                var courseProgressRecords = await _context.CourseProgresses
                    .Where(cp => cp.CourseId == request.Id)
                    .ToListAsync(cancellationToken);

                if (courseProgressRecords.Any())
                {
                    _context.CourseProgresses.RemoveRange(courseProgressRecords);
                }

                // Then delete the course
                _context.Courses.Remove(course);
                
                var changes = await _context.SaveChangesAsync(cancellationToken);

                return changes > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
