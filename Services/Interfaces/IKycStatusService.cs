using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Services.Interfaces
{
    /// <summary>
    /// Service for managing and correcting KYC statuses
    /// </summary>
    public interface IKycStatusService
    {
        /// <summary>
        /// Corrects KYC statuses for all users based on their document submissions
        /// </summary>
        Task CorrectAllUserKycStatusesAsync();

        /// <summary>
        /// Calculates the correct KYC status for a user based on their documents
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <returns>The correct KYC status</returns>
        Task<KYCStatus> CalculateCorrectKycStatusAsync(string userId);

        /// <summary>
        /// Calculates the correct KYC status based on a collection of documents
        /// </summary>
        /// <param name="documents">The user's KYC documents</param>
        /// <returns>The correct KYC status</returns>
        KYCStatus CalculateCorrectKycStatus(ICollection<KYCDocument> documents);

        /// <summary>
        /// Updates a specific user's KYC status based on their documents
        /// </summary>
        /// <param name="userId">The user ID to update</param>
        /// <returns>True if the status was updated, false if no change was needed</returns>
        Task<bool> UpdateUserKycStatusAsync(string userId);

        /// <summary>
        /// Gets the count of users with incorrect KYC statuses (for monitoring)
        /// </summary>
        Task<int> GetIncorrectStatusCountAsync();

        /// <summary>
        /// Gets a report of users with incorrect KYC statuses
        /// </summary>
        Task<List<(string UserId, string Email, KYCStatus CurrentStatus, KYCStatus ExpectedStatus)>> GetIncorrectStatusReportAsync();
    }
}