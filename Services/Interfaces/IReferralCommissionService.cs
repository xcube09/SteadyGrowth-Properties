using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces
{
    /// <summary>
    /// Service for managing referral commissions when packages are purchased
    /// </summary>
    public interface IReferralCommissionService
    {
        /// <summary>
        /// Process referral commissions when a package is purchased.
        /// Credits Level 1 (direct referrer) and Level 2 (indirect referrer) commissions.
        /// </summary>
        /// <param name="userId">The user who purchased the package</param>
        /// <param name="packageId">The package that was purchased</param>
        /// <returns>List of ReferralCommission records created</returns>
        Task<List<ReferralCommission>> ProcessPackagePurchaseCommissionsAsync(string userId, int packageId);

        /// <summary>
        /// Get commission rate for a specific level and package
        /// </summary>
        /// <param name="level">1 = Direct referrer, 2 = Indirect referrer</param>
        /// <param name="packageId">The academy package ID</param>
        /// <returns>Commission amount or null if not configured</returns>
        Task<decimal?> GetCommissionRateAsync(int level, int packageId);

        /// <summary>
        /// Get all commission levels with their rates
        /// </summary>
        Task<List<ReferralCommissionLevel>> GetAllCommissionLevelsAsync();

        /// <summary>
        /// Update commission rate for a specific level and package
        /// </summary>
        Task<ReferralCommissionLevel?> UpdateCommissionRateAsync(int level, int packageId, decimal newAmount);

        /// <summary>
        /// Get commissions earned by a user
        /// </summary>
        Task<List<ReferralCommission>> GetUserCommissionsAsync(string userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get total commissions earned by a user
        /// </summary>
        Task<decimal> GetTotalCommissionsEarnedAsync(string userId);
    }
}
