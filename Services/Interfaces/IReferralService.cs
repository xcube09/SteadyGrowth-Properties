using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces;

/// <summary>
/// Service interface for managing user referrals and commissions.
/// </summary>
public interface IReferralService
{
    /// <summary>
    /// Processes a new referral using a referral code and the new user's ID.
    /// </summary>
    /// <param name="referrerCode">The referral code of the referrer.</param>
    /// <param name="newUserId">The ID of the newly registered user.</param>
    /// <returns>True if processed successfully, otherwise false.</returns>
    Task<bool> ProcessReferralAsync(string referrerCode, string newUserId);

    /// <summary>
    /// Calculates the total commission earned by a referrer.
    /// </summary>
    /// <param name="referrerId">The user ID of the referrer.</param>
    /// <returns>The total commission earned.</returns>
    Task<decimal> CalculateCommissionAsync(string referrerId);

    /// <summary>
    /// Gets all referrals made by a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of referrals.</returns>
    Task<IEnumerable<Referral>> GetUserReferralsAsync(string userId);

    /// <summary>
    /// Generates a unique referral code for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The generated referral code.</returns>
    Task<string> GenerateReferralCodeAsync(string userId);

    /// <summary>
    /// Gets referral statistics for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>Referral statistics.</returns>
    Task<ReferralStats> GetReferralStatsAsync(string userId);

    /// <summary>
    /// Processes commission payment for a referral.
    /// </summary>
    /// <param name="referralId">The referral ID.</param>
    /// <returns>True if payment processed, otherwise false.</returns>
    Task<bool> ProcessCommissionPaymentAsync(int referralId);

    /// <summary>
    /// Validates if a referral code exists and is active.
    /// </summary>
    /// <param name="referralCode">The referral code to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    Task<bool> ValidateReferralCodeAsync(string referralCode);

    /// <summary>
    /// Gets the user associated with a referral code.
    /// </summary>
    /// <param name="referralCode">The referral code.</param>
    /// <returns>The user if found, otherwise null.</returns>
    Task<User?> GetUserByReferralCodeAsync(string referralCode);
}

/// <summary>
/// Statistics for a user's referrals and commissions.
/// </summary>
public class ReferralStats
{
    /// <summary>
    /// Total number of referrals made by the user.
    /// </summary>
    public int TotalReferrals { get; set; }

    /// <summary>
    /// Number of active referrals.
    /// </summary>
    public int ActiveReferrals { get; set; }

    /// <summary>
    /// Total commission earned by the user.
    /// </summary>
    public decimal TotalCommissionEarned { get; set; }

    /// <summary>
    /// Commission that is pending payment.
    /// </summary>
    public decimal PendingCommission { get; set; }

    /// <summary>
    /// Commission that has been paid.
    /// </summary>
    public decimal CommissionPaid { get; set; }

    /// <summary>
    /// The user's referral code.
    /// </summary>
    public string? ReferralCode { get; set; }
}
