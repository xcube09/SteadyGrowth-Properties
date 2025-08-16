using SteadyGrowth.Web.Models.Entities;
using System.Security.Claims;

namespace SteadyGrowth.Web.Services.Interfaces
{
    /// <summary>
    /// Service for retrieving information about the currently logged-in user
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the current user's ID
        /// </summary>
        /// <returns>User ID or null if not authenticated</returns>
        string? GetUserId();

        /// <summary>
        /// Gets the current user's email address
        /// </summary>
        /// <returns>User email or null if not authenticated</returns>
        string? GetUserEmail();

        /// <summary>
        /// Gets the current user's username
        /// </summary>
        /// <returns>Username or null if not authenticated</returns>
        string? GetUserName();

        /// <summary>
        /// Gets the full User entity for the current user
        /// </summary>
        /// <returns>User entity or null if not authenticated</returns>
        Task<User?> GetCurrentUserAsync();

        /// <summary>
        /// Gets the full User entity with related data for the current user
        /// </summary>
        /// <param name="includeWallet">Include wallet information</param>
        /// <param name="includePackage">Include academy package information</param>
        /// <returns>User entity with related data or null if not authenticated</returns>
        Task<User?> GetCurrentUserWithDetailsAsync(bool includeWallet = false, bool includePackage = false);

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        bool IsAuthenticated();

        /// <summary>
        /// Checks if the current user is in the specified role
        /// </summary>
        /// <param name="role">Role name to check</param>
        /// <returns>True if user is in role, false otherwise</returns>
        Task<bool> IsInRoleAsync(string role);

        /// <summary>
        /// Checks if the current user is in any of the specified roles
        /// </summary>
        /// <param name="roles">Array of role names to check</param>
        /// <returns>True if user is in any role, false otherwise</returns>
        Task<bool> IsInAnyRoleAsync(params string[] roles);

        /// <summary>
        /// Gets all claims for the current user
        /// </summary>
        /// <returns>Collection of user claims</returns>
        IEnumerable<Claim> GetUserClaims();

        /// <summary>
        /// Gets a specific claim value for the current user
        /// </summary>
        /// <param name="claimType">Type of claim to retrieve</param>
        /// <returns>Claim value or null if not found</returns>
        string? GetClaimValue(string claimType);

        /// <summary>
        /// Gets the current user's full name (FirstName + LastName)
        /// </summary>
        /// <returns>Full name or null if not authenticated</returns>
        Task<string?> GetUserFullNameAsync();

        /// <summary>
        /// Checks if the current user has completed KYC
        /// </summary>
        /// <returns>True if KYC is completed, false otherwise</returns>
        Task<bool> IsKycCompletedAsync();

        /// <summary>
        /// Gets the current user's referral code
        /// </summary>
        /// <returns>Referral code or null if not available</returns>
        Task<string?> GetUserReferralCodeAsync();

        /// <summary>
        /// Gets the current user's wallet balance
        /// </summary>
        /// <returns>Wallet balance or 0 if not available</returns>
        Task<decimal> GetWalletBalanceAsync();

        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        /// <returns>True if user is admin, false otherwise</returns>
        Task<bool> IsAdminAsync();

        /// <summary>
        /// Gets the current HTTP context
        /// </summary>
        /// <returns>HttpContext or null</returns>
        HttpContext? GetHttpContext();

        /// <summary>
        /// Checks if currently impersonating another user
        /// </summary>
        /// <returns>True if impersonating, false otherwise</returns>
        bool IsImpersonating();

        /// <summary>
        /// Gets the actual admin user ID when impersonating
        /// </summary>
        /// <returns>Admin user ID or null if not impersonating</returns>
        string? GetActualAdminUserId();

        /// <summary>
        /// Starts impersonating the specified user
        /// </summary>
        /// <param name="userId">User ID to impersonate</param>
        void StartImpersonation(string userId);

        /// <summary>
        /// Stops the current impersonation
        /// </summary>
        void StopImpersonation();

        /// <summary>
        /// Gets the impersonated user information
        /// </summary>
        /// <returns>Impersonated user or null if not impersonating</returns>
        Task<User?> GetImpersonatedUserAsync();
    }
}