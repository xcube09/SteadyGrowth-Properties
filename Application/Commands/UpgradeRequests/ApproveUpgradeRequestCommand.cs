using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

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
        private readonly IReferralCommissionService _referralCommissionService;
        private readonly ILogger<ApproveUpgradeRequestCommandHandler> _logger;

        public ApproveUpgradeRequestCommandHandler(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IReferralCommissionService referralCommissionService,
            ILogger<ApproveUpgradeRequestCommandHandler> logger)
        {
            _context = context;
            _userManager = userManager;
            _referralCommissionService = referralCommissionService;
            _logger = logger;
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

                // Process referral commissions after package assignment
                try
                {
                    var commissions = await _referralCommissionService.ProcessPackagePurchaseCommissionsAsync(
                        user.Id,
                        upgradeRequest.RequestedPackageId);

                    _logger.LogInformation(
                        "Processed {Count} referral commissions for upgrade request {RequestId}",
                        commissions.Count, request.RequestId);
                }
                catch (Exception ex)
                {
                    // Log but don't fail the approval if commission processing fails
                    _logger.LogError(ex,
                        "Error processing referral commissions for upgrade request {RequestId}. Package was still assigned.",
                        request.RequestId);
                }

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