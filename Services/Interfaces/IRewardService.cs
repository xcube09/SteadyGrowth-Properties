using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces;

/// <summary>
/// Service interface for managing user rewards and points.
/// </summary>
public interface IRewardService
{
    /// <summary>
    /// Adds reward points to a user's account.
    /// </summary>
    Task<bool> AddRewardPointsAsync(string userId, int points, string description, RewardType rewardType);

    /// <summary>
    /// Redeems reward points for a monetary value.
    /// </summary>
    Task<bool> RedeemPointsAsync(string userId, int points, decimal moneyValue);

    /// <summary>
    /// Gets all rewards for a user.
    /// </summary>
    Task<IEnumerable<Reward>> GetUserRewardsAsync(string userId);

    /// <summary>
    /// Calculates the monetary value of a given number of points.
    /// </summary>
    Task<decimal> CalculateRewardValueAsync(int points);

    /// <summary>
    /// Gets the total reward points for a user.
    /// </summary>
    Task<int> GetUserTotalPointsAsync(string userId);
}
