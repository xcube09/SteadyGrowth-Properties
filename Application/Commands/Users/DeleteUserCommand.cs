using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Application.Commands.Users
{
    public class DeleteUserCommand : IRequest<bool>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public DeleteUserCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var user = await _context.Users
                    .Include(u => u.Wallet)
                    .Include(u => u.Properties)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                if (user == null)
                {
                    return false;
                }

                // Delete related entities in the correct order to avoid foreign key violations
                
                // 1. Delete CourseProgress records
                var courseProgresses = await _context.CourseProgresses
                    .Where(cp => cp.UserId == request.UserId)
                    .ToListAsync(cancellationToken);
                if (courseProgresses.Any())
                {
                    _context.CourseProgresses.RemoveRange(courseProgresses);
                }

                // 2. Delete SegmentProgress records
                var segmentProgresses = await _context.SegmentProgresses
                    .Where(sp => sp.UserId == request.UserId)
                    .ToListAsync(cancellationToken);
                if (segmentProgresses.Any())
                {
                    _context.SegmentProgresses.RemoveRange(segmentProgresses);
                }

                // 3. Delete PropertyCommissions (both as user and as admin who added them)
                var propertyCommissions = await _context.PropertyCommissions
                    .Where(pc => pc.UserId == request.UserId || pc.AddedByUserId == request.UserId)
                    .ToListAsync(cancellationToken);
                if (propertyCommissions.Any())
                {
                    _context.PropertyCommissions.RemoveRange(propertyCommissions);
                }

                // 4. Delete Wallet and related transactions
                if (user.Wallet != null)
                {
                    // Delete wallet transactions
                    var walletTransactions = await _context.WalletTransactions
                        .Where(wt => wt.WalletId == user.Wallet.Id)
                        .ToListAsync(cancellationToken);
                    if (walletTransactions.Any())
                    {
                        _context.WalletTransactions.RemoveRange(walletTransactions);
                    }

                    // Delete withdrawal requests
                    var withdrawalRequests = await _context.WithdrawalRequests
                        .Where(wr => wr.UserId == request.UserId)
                        .ToListAsync(cancellationToken);
                    if (withdrawalRequests.Any())
                    {
                        _context.WithdrawalRequests.RemoveRange(withdrawalRequests);
                    }

                    // Delete the wallet
                    _context.Wallets.Remove(user.Wallet);
                }

                // 5. Delete Referrals (both made and received)
                var referralsMade = await _context.Referrals
                    .Where(r => r.ReferrerId == request.UserId)
                    .ToListAsync(cancellationToken);
                if (referralsMade.Any())
                {
                    _context.Referrals.RemoveRange(referralsMade);
                }

                var referralsReceived = await _context.Referrals
                    .Where(r => r.ReferredUserId == request.UserId)
                    .ToListAsync(cancellationToken);
                if (referralsReceived.Any())
                {
                    _context.Referrals.RemoveRange(referralsReceived);
                }

                // 6. Delete Rewards
                var rewards = await _context.Rewards
                    .Where(r => r.UserId == request.UserId)
                    .ToListAsync(cancellationToken);
                if (rewards.Any())
                {
                    _context.Rewards.RemoveRange(rewards);
                }

                // 7. Delete UserActivities
                var userActivities = await _context.UserActivities
                    .Where(ua => ua.UserId == request.UserId)
                    .ToListAsync(cancellationToken);
                if (userActivities.Any())
                {
                    _context.UserActivities.RemoveRange(userActivities);
                }

                // 8. Delete UpgradeRequests
                var upgradeRequests = await _context.UpgradeRequests
                    .Where(ur => ur.UserId == request.UserId)
                    .ToListAsync(cancellationToken);
                if (upgradeRequests.Any())
                {
                    _context.UpgradeRequests.RemoveRange(upgradeRequests);
                }

                // 9. Delete KYCDocuments
                var kycDocuments = await _context.KYCDocuments
                    .Where(kyc => kyc.UserId == request.UserId)
                    .ToListAsync(cancellationToken);
                if (kycDocuments.Any())
                {
                    _context.KYCDocuments.RemoveRange(kycDocuments);
                }

                // 10. Handle Properties - Update UserId to null instead of deleting properties
                // This preserves property data while removing the user reference
                var properties = await _context.Properties
                    .Where(p => p.UserId == request.UserId)
                    .ToListAsync(cancellationToken);
                foreach (var property in properties)
                {
                    property.UserId = null; // Remove user reference but keep property
                }

                // 11. Delete VettingLogs where user was the admin
                var vettingLogs = await _context.VettingLogs
                    .Where(vl => vl.AdminUserId == request.UserId)
                    .ToListAsync(cancellationToken);
                foreach (var log in vettingLogs)
                {
                    log.AdminUserId = null; // Remove admin reference
                }

                // 12. Finally, delete the user
                _context.Users.Remove(user);

                var changes = await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return changes > 0;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }
    }
}