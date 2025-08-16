using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Application.Commands.UpgradeRequests
{
    public class RejectUpgradeRequestCommand : IRequest<bool>
    {
        public int RequestId { get; set; }
        public string AdminUserId { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }

    public class RejectUpgradeRequestCommandHandler : IRequestHandler<RejectUpgradeRequestCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public RejectUpgradeRequestCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(RejectUpgradeRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var upgradeRequest = await _context.UpgradeRequests
                    .FirstOrDefaultAsync(ur => ur.Id == request.RequestId, cancellationToken);

                if (upgradeRequest == null || upgradeRequest.Status != UpgradeRequestStatus.Pending)
                {
                    return false;
                }

                // Update the upgrade request status
                upgradeRequest.Status = UpgradeRequestStatus.Rejected;
                upgradeRequest.ProcessedAt = DateTime.UtcNow;
                
                // Store rejection reason in payment details for now (could be separate field)
                if (!string.IsNullOrWhiteSpace(request.RejectionReason))
                {
                    upgradeRequest.PaymentDetails += $" | Rejection Reason: {request.RejectionReason}";
                }

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}