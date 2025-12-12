using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Services.Implementations
{
    /// <summary>
    /// Service for managing referral commissions when packages are purchased
    /// </summary>
    public class ReferralCommissionService : IReferralCommissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWalletService _walletService;
        private readonly ILogger<ReferralCommissionService> _logger;

        public ReferralCommissionService(
            ApplicationDbContext context,
            IWalletService walletService,
            ILogger<ReferralCommissionService> logger)
        {
            _context = context;
            _walletService = walletService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<List<ReferralCommission>> ProcessPackagePurchaseCommissionsAsync(string userId, int packageId)
        {
            var commissions = new List<ReferralCommission>();

            try
            {
                // Get the user who purchased the package
                var purchaser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (purchaser == null)
                {
                    _logger.LogWarning("User {UserId} not found for commission processing", userId);
                    return commissions;
                }

                // Get the package details
                var package = await _context.AcademyPackages
                    .FirstOrDefaultAsync(p => p.Id == packageId);

                if (package == null)
                {
                    _logger.LogWarning("Package {PackageId} not found for commission processing", packageId);
                    return commissions;
                }

                // Process Level 1 (Direct Referrer) commission
                if (!string.IsNullOrEmpty(purchaser.ReferredBy))
                {
                    var level1Commission = await ProcessCommissionForLevelAsync(
                        beneficiaryUserId: purchaser.ReferredBy,
                        sourceUserId: userId,
                        packageId: packageId,
                        level: 1,
                        packageName: package.Name);

                    if (level1Commission != null)
                    {
                        commissions.Add(level1Commission);

                        // Process Level 2 (Indirect Referrer) commission
                        var level1Referrer = await _context.Users
                            .FirstOrDefaultAsync(u => u.Id == purchaser.ReferredBy);

                        if (level1Referrer != null && !string.IsNullOrEmpty(level1Referrer.ReferredBy))
                        {
                            var level2Commission = await ProcessCommissionForLevelAsync(
                                beneficiaryUserId: level1Referrer.ReferredBy,
                                sourceUserId: userId,
                                packageId: packageId,
                                level: 2,
                                packageName: package.Name);

                            if (level2Commission != null)
                            {
                                commissions.Add(level2Commission);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Processed {Count} referral commissions for user {UserId} purchasing package {PackageId}",
                    commissions.Count, userId, packageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing referral commissions for user {UserId} and package {PackageId}",
                    userId, packageId);
                throw;
            }

            return commissions;
        }

        private async Task<ReferralCommission?> ProcessCommissionForLevelAsync(
            string beneficiaryUserId,
            string sourceUserId,
            int packageId,
            int level,
            string packageName)
        {
            // Get commission rate for this level and package
            var commissionAmount = await GetCommissionRateAsync(level, packageId);

            if (!commissionAmount.HasValue || commissionAmount.Value <= 0)
            {
                _logger.LogDebug("No commission rate configured for Level {Level}, Package {PackageId}",
                    level, packageId);
                return null;
            }

            // Credit the beneficiary's wallet
            var levelDescription = level == 1 ? "Direct" : "Indirect";
            var description = $"Level {level} ({levelDescription}) referral commission for {packageName} purchase";
            var reference = $"REF-L{level}-{sourceUserId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var walletTransaction = await _walletService.AddCommissionAsync(
                beneficiaryUserId,
                commissionAmount.Value,
                description,
                reference);

            // Create commission record
            var commission = new ReferralCommission
            {
                BeneficiaryUserId = beneficiaryUserId,
                SourceUserId = sourceUserId,
                AcademyPackageId = packageId,
                Level = level,
                CommissionAmount = commissionAmount.Value,
                WalletTransactionId = walletTransaction?.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.ReferralCommissions.Add(commission);

            _logger.LogInformation(
                "Created Level {Level} commission of ${Amount} for user {BeneficiaryId} from user {SourceId} purchasing package {PackageId}",
                level, commissionAmount.Value, beneficiaryUserId, sourceUserId, packageId);

            return commission;
        }

        /// <inheritdoc />
        public async Task<decimal?> GetCommissionRateAsync(int level, int packageId)
        {
            var commissionLevel = await _context.ReferralCommissionLevels
                .FirstOrDefaultAsync(cl => cl.Level == level &&
                                          cl.AcademyPackageId == packageId &&
                                          cl.IsActive);

            return commissionLevel?.CommissionAmount;
        }

        /// <inheritdoc />
        public async Task<List<ReferralCommissionLevel>> GetAllCommissionLevelsAsync()
        {
            return await _context.ReferralCommissionLevels
                .Include(cl => cl.AcademyPackage)
                .OrderBy(cl => cl.Level)
                .ThenBy(cl => cl.AcademyPackageId)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<ReferralCommissionLevel?> UpdateCommissionRateAsync(int level, int packageId, decimal newAmount)
        {
            var commissionLevel = await _context.ReferralCommissionLevels
                .FirstOrDefaultAsync(cl => cl.Level == level && cl.AcademyPackageId == packageId);

            if (commissionLevel == null)
            {
                return null;
            }

            commissionLevel.CommissionAmount = newAmount;
            commissionLevel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated commission rate for Level {Level}, Package {PackageId} to ${Amount}",
                level, packageId, newAmount);

            return commissionLevel;
        }

        /// <inheritdoc />
        public async Task<List<ReferralCommission>> GetUserCommissionsAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _context.ReferralCommissions
                .Where(c => c.BeneficiaryUserId == userId)
                .Include(c => c.SourceUser)
                .Include(c => c.AcademyPackage)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<decimal> GetTotalCommissionsEarnedAsync(string userId)
        {
            return await _context.ReferralCommissions
                .Where(c => c.BeneficiaryUserId == userId)
                .SumAsync(c => c.CommissionAmount);
        }
    }
}
