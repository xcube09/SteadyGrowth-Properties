using MediatR;
using System.Threading;
using System.Threading.Tasks;
using SteadyGrowth.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Commands.Academy
{
    public class MarkLessonCompleteCommand : IRequest<bool>
    {
        public string UserId { get; set; } = string.Empty;
        public int CourseId { get; set; }
    }

    public class MarkLessonCompleteCommandHandler : IRequestHandler<MarkLessonCompleteCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public MarkLessonCompleteCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(MarkLessonCompleteCommand request, CancellationToken cancellationToken)
        {
            var progress = await _context.CourseProgresses
                .Include(cp => cp.Course)
                .FirstOrDefaultAsync(cp => cp.UserId == request.UserId && cp.CourseId == request.CourseId, cancellationToken);

            if (progress == null)
            {
                return false; // Course not started
            }

            if (progress.CompletedLessonsCount < progress.Course.TotalLessons)
            {
                progress.CompletedLessonsCount++;
                progress.LastAccessedDate = DateTime.UtcNow;

                if (progress.CompletedLessonsCount == progress.Course.TotalLessons)
                {
                    progress.IsCompleted = true;
                }

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false; // All lessons already completed
        }
    }
}
