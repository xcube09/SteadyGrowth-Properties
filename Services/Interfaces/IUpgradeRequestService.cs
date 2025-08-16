using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing academy upgrade requests.
    /// </summary>
    public interface IUpgradeRequestService
    {
        /// <summary>
        /// Gets the count of pending upgrade requests.
        /// </summary>
        Task<int> GetPendingUpgradeRequestsCountAsync();

        /// <summary>
        /// Gets all pending upgrade requests.
        /// </summary>
        Task<IList<UpgradeRequest>> GetPendingUpgradeRequestsAsync();

        /// <summary>
        /// Gets upgrade requests with filtering options.
        /// </summary>
        Task<IList<UpgradeRequest>> GetUpgradeRequestsAsync(
            UpgradeRequestStatus? status = null,
            string? searchTerm = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 1,
            int pageSize = 20);

        /// <summary>
        /// Gets an upgrade request by ID.
        /// </summary>
        Task<UpgradeRequest?> GetUpgradeRequestByIdAsync(int id);

        /// <summary>
        /// Approves an upgrade request and upgrades the user's package.
        /// </summary>
        Task<bool> ApproveUpgradeRequestAsync(int requestId, string adminUserId);

        /// <summary>
        /// Rejects an upgrade request with an optional reason.
        /// </summary>
        Task<bool> RejectUpgradeRequestAsync(int requestId, string adminUserId, string? rejectionReason = null);

        /// <summary>
        /// Gets upgrade request statistics.
        /// </summary>
        Task<UpgradeRequestStats> GetUpgradeRequestStatsAsync();
    }

    /// <summary>
    /// Statistics for upgrade requests.
    /// </summary>
    public class UpgradeRequestStats
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public decimal ApprovalRate => TotalRequests > 0 ? (decimal)ApprovedRequests / TotalRequests * 100 : 0;
    }
}