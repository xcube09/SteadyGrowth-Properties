using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Services.Implementations
{
    /// <summary>
    /// Service for managing and correcting KYC statuses
    /// </summary>
    public class KycStatusService : IKycStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<KycStatusService> _logger;

        public KycStatusService(
            ApplicationDbContext context,
            UserManager<User> userManager,
            ILogger<KycStatusService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task CorrectAllUserKycStatusesAsync()
        {
            _logger.LogInformation("Starting KYC status correction for all users");

            // Focus on users with NotStarted status who might have documents
            var usersToCheck = await _context.Users
                .Where(u => u.KYCStatus == KYCStatus.NotStarted)
                .Include(u => u.KYCDocuments)
                .ToListAsync();

            var correctedCount = 0;

            foreach (var user in usersToCheck)
            {
                var correctedStatus = CalculateCorrectKycStatus(user.KYCDocuments);
                if (correctedStatus != user.KYCStatus)
                {
                    _logger.LogInformation("Correcting KYC status for user {UserId} ({Email}) from {CurrentStatus} to {NewStatus}",
                        user.Id, user.Email, user.KYCStatus, correctedStatus);

                    user.KYCStatus = correctedStatus;
                    correctedCount++;
                }
            }

            if (correctedCount > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("KYC status correction completed. {CorrectedCount} users had their status corrected", correctedCount);
            }
            else
            {
                _logger.LogInformation("KYC status correction completed. No corrections needed");
            }
        }

        public async Task<KYCStatus> CalculateCorrectKycStatusAsync(string userId)
        {
            var documents = await _context.KYCDocuments
                .Where(d => d.UserId == userId)
                .ToListAsync();

            return CalculateCorrectKycStatus(documents);
        }

        public KYCStatus CalculateCorrectKycStatus(ICollection<KYCDocument> documents)
        {
            // If no documents at all, status is NotStarted
            if (!documents.Any())
            {
                return KYCStatus.NotStarted;
            }

            bool hasApprovedIdentity = documents.Any(d =>
                (d.DocumentType == DocumentType.NationalID ||
                 d.DocumentType == DocumentType.Passport ||
                 d.DocumentType == DocumentType.DriversLicense) &&
                d.Status == DocumentStatus.Approved);

            bool hasApprovedProofOfAddress = documents.Any(d =>
                (d.DocumentType == DocumentType.UtilityBill ||
                 d.DocumentType == DocumentType.ProofOfAddress) &&
                d.Status == DocumentStatus.Approved);

            // Full approval - both document types approved
            if (hasApprovedIdentity && hasApprovedProofOfAddress)
            {
                return KYCStatus.Approved;
            }
            // Any document rejected - overall status is rejected
            else if (documents.Any(d => d.Status == DocumentStatus.Rejected))
            {
                return KYCStatus.Rejected;
            }
            // Documents exist (submitted/approved) - keep as Submitted
            else if (documents.Any(d => d.Status == DocumentStatus.Pending || d.Status == DocumentStatus.Approved))
            {
                return KYCStatus.Submitted;
            }
            // Should never reach here if documents exist, but safety fallback
            else
            {
                return KYCStatus.NotStarted;
            }
        }

        public async Task<bool> UpdateUserKycStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for KYC status update", userId);
                return false;
            }

            var documents = await _context.KYCDocuments
                .Where(d => d.UserId == userId)
                .ToListAsync();

            var correctStatus = CalculateCorrectKycStatus(documents);

            if (correctStatus != user.KYCStatus)
            {
                _logger.LogInformation("Updating KYC status for user {UserId} from {CurrentStatus} to {NewStatus}",
                    userId, user.KYCStatus, correctStatus);

                user.KYCStatus = correctStatus;
                await _userManager.UpdateAsync(user);
                return true;
            }

            return false;
        }

        public async Task<int> GetIncorrectStatusCountAsync()
        {
            var incorrectCount = 0;
            var users = await _context.Users
                .Include(u => u.KYCDocuments)
                .ToListAsync();

            foreach (var user in users)
            {
                var expectedStatus = CalculateCorrectKycStatus(user.KYCDocuments);
                if (expectedStatus != user.KYCStatus)
                {
                    incorrectCount++;
                }
            }

            return incorrectCount;
        }

        public async Task<List<(string UserId, string Email, KYCStatus CurrentStatus, KYCStatus ExpectedStatus)>> GetIncorrectStatusReportAsync()
        {
            var incorrectStatuses = new List<(string UserId, string Email, KYCStatus CurrentStatus, KYCStatus ExpectedStatus)>();
            var users = await _context.Users
                .Include(u => u.KYCDocuments)
                .ToListAsync();

            foreach (var user in users)
            {
                var expectedStatus = CalculateCorrectKycStatus(user.KYCDocuments);
                if (expectedStatus != user.KYCStatus)
                {
                    incorrectStatuses.Add((user.Id, user.Email ?? "N/A", user.KYCStatus, expectedStatus));
                }
            }

            return incorrectStatuses;
        }
    }
}