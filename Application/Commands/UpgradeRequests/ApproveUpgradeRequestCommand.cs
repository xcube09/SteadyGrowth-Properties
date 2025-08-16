using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Application.Commands.UpgradeRequests
{
    public class ApproveUpgradeRequestCommand : IRequest<bool>
    {
        public int RequestId { get; set; }
        public string AdminUserId { get; set; } = string.Empty;
    }

    public class ApproveUpgradeRequestCommandHandler : IRequestHandler<ApproveUpgradeRequestCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ApproveUpgradeRequestCommandHandler(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> Handle(ApproveUpgradeRequestCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Get the upgrade request with user and package details
                var upgradeRequest = await _context.UpgradeRequests
                    .Include(ur => ur.User)
                    .Include(ur => ur.RequestedPackage)
                    .FirstOrDefaultAsync(ur => ur.Id == request.RequestId, cancellationToken);

                if (upgradeRequest == null || upgradeRequest.Status != UpgradeRequestStatus.Pending)
                {
                    return false;
                }

                // Update the upgrade request status
                upgradeRequest.Status = UpgradeRequestStatus.Approved;
                upgradeRequest.ProcessedAt = DateTime.UtcNow;

                // Update the user's academy package
                var user = upgradeRequest.User;
                user.AcademyPackageId = upgradeRequest.RequestedPackageId;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }
    }
}