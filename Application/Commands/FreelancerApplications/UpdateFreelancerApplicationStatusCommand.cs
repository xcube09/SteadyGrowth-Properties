using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Application.Commands.FreelancerApplications
{
    public class UpdateFreelancerApplicationStatusCommand : IRequest<bool>
    {
        public int ApplicationId { get; set; }
        public FreelancerApplicationStatus NewStatus { get; set; }
        public string? AdminNotes { get; set; }
        public string? ReviewedByUserId { get; set; }
    }

    public class UpdateFreelancerApplicationStatusCommandHandler : IRequestHandler<UpdateFreelancerApplicationStatusCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public UpdateFreelancerApplicationStatusCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateFreelancerApplicationStatusCommand request, CancellationToken cancellationToken)
        {
            var application = await _context.FreelancerApplications
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (application == null)
            {
                return false;
            }

            application.Status = request.NewStatus;
            application.AdminNotes = request.AdminNotes;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedByUserId = request.ReviewedByUserId;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
