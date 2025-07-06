using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces;

/// <summary>
/// Service interface for user management and statistics.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user by their unique ID.
    /// </summary>
    Task<User?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Updates a user's information.
    /// </summary>
    Task<bool> UpdateUserAsync(User user);

    /// <summary>
    /// Gets statistics for a user.
    /// </summary>
    Task<UserStats> GetUserStatsAsync(string userId);

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    Task<bool> DeactivateUserAsync(string userId);

    /// <summary>
    /// Gets all users with paging support.
    /// </summary>
    // Task<IEnumerable<User>> GetAllUsersAsync(int page = 1, int pageSize = 50);

    /// <summary>
    /// Gets the total number of users in the system.
    /// </summary>
    Task<int> GetTotalUsersCountAsync();
}

/// <summary>
/// Statistics for a user account.
/// </summary>
public class UserStats
{
    public int TotalProperties { get; set; }
    public int ApprovedProperties { get; set; }
    public int PendingProperties { get; set; }
    public int TotalReferrals { get; set; }
    public int TotalRewardPoints { get; set; }
    public DateTime JoinDate { get; set; }
}
