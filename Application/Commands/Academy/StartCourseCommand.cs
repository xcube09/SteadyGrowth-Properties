using MediatR;
using System.Threading;
using System.Threading.Tasks;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Commands.Academy
{
    public class StartCourseCommand : IRequest<bool>
    {
        public string UserId { get; set; } = string.Empty;
        public int CourseId { get; set; }
    }

    public class StartCourseCommandHandler : IRequestHandler<StartCourseCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public StartCourseCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(StartCourseCommand request, CancellationToken cancellationToken)
        {
            var existingProgress = await _context.CourseProgresses
                .FirstOrDefaultAsync(cp => cp.UserId == request.UserId && cp.CourseId == request.CourseId, cancellationToken);

            if (existingProgress == null)
            {
                var course = await _context.Courses.FindAsync(new object[] { request.CourseId }, cancellationToken);
                if (course == null)
                {
                    return false; // Course not found
                }

                var newProgress = new CourseProgress
                {
                    UserId = request.UserId,
                    CourseId = request.CourseId,
                    CompletedLessonsCount = 0,
                    IsCompleted = false,
                    LastAccessedDate = DateTime.UtcNow
                };
                _context.CourseProgresses.Add(newProgress);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false; // Course already started
        }
    }
}
