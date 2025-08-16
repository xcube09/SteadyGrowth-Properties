using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Services.Implementations;

/// <summary>
/// Service for managing user referrals, codes, and commissions.
/// </summary>
public class ReferralService : IReferralService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReferralService> _logger;
    private readonly IConfiguration _config;
    private readonly ICurrencyService _currencyService;
    private readonly decimal _commissionPercent;
    private const int MaxReferralDepth = 3;
    private const int ReferralCodeLength = 8;

    public ReferralService(ApplicationDbContext db, ILogger<ReferralService> logger, IConfiguration config, ICurrencyService currencyService)
    {
        _db = db;
        _logger = logger;
        _config = config;
        _currencyService = currencyService;
        _commissionPercent = _config.GetValue<decimal>("Referral:CommissionPercent", 0.05m);
    }

    public async Task<bool> ProcessReferralAsync(string referrerCode, string newUserId)
    {
        try
        {
            _logger.LogInformation("Starting referral processing for code '{ReferrerCode}' and new user '{NewUserId}'", 
                referrerCode, newUserId);

            // Check if referrer exists by code
            var referrer = await _db.Users.FirstOrDefaultAsync(u => u.ReferralCode == referrerCode).ConfigureAwait(false);
            if (referrer == null)
            {
                _logger.LogWarning("Referrer not found for code '{ReferrerCode}'. Checking if any users have referral codes...", referrerCode);
                
                var totalUsersWithCodes = await _db.Users.CountAsync(u => !string.IsNullOrEmpty(u.ReferralCode)).ConfigureAwait(false);
                _logger.LogWarning("Total users with referral codes in database: {Count}", totalUsersWithCodes);
                
                if (totalUsersWithCodes > 0)
                {
                    var sampleCodes = await _db.Users
                        .Where(u => !string.IsNullOrEmpty(u.ReferralCode))
                        .Take(5)
                        .Select(u => u.ReferralCode)
                        .ToListAsync().ConfigureAwait(false);
                    _logger.LogWarning("Sample referral codes in database: {SampleCodes}", string.Join(", ", sampleCodes));
                }
                
                return false;
            }

            _logger.LogInformation("Found referrer: ID={ReferrerId}, Email={ReferrerEmail}", referrer.Id, referrer.Email);

            // Check if new user exists
            var newUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == newUserId).ConfigureAwait(false);
            if (newUser == null)
            {
                _logger.LogWarning("New user '{NewUserId}' not found in database. Checking total user count...", newUserId);
                
                var totalUsers = await _db.Users.CountAsync().ConfigureAwait(false);
                _logger.LogWarning("Total users in database: {Count}", totalUsers);
                
                // Check if user might be in Identity tables but not in our context
                var recentUsers = await _db.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .Select(u => new { u.Id, u.Email, u.CreatedAt })
                    .ToListAsync().ConfigureAwait(false);
                
                _logger.LogWarning("Recent users in database: {RecentUsers}", 
                    string.Join(", ", recentUsers.Select(u => $"{u.Email}({u.Id})")));
                
                return false;
            }

            _logger.LogInformation("Found new user: ID={NewUserId}, Email={NewUserEmail}, CreatedAt={CreatedAt}", 
                newUser.Id, newUser.Email, newUser.CreatedAt);

            // Check for self-referral
            if (referrer.Id == newUserId)
            {
                _logger.LogWarning("Self-referral attempt detected for user {UserId}", newUserId);
                return false;
            }

            // Prevent circular references (check up to MaxReferralDepth)
            var current = referrer;
            for (int i = 0; i < MaxReferralDepth; i++)
            {
                if (current.ReferredBy == newUserId)
                {
                    _logger.LogWarning("Circular referral detected for user {UserId}", newUserId);
                    return false;
                }
                if (string.IsNullOrEmpty(current.ReferredBy)) break;
                current = await _db.Users.FirstOrDefaultAsync(u => u.Id == current.ReferredBy).ConfigureAwait(false);
                if (current == null) break;
            }

            // Only allow if newUser has not already been referred
            if (!string.IsNullOrEmpty(newUser.ReferredBy))
            {
                _logger.LogWarning("User {UserId} already has a referrer: {ExistingReferrer}", newUserId, newUser.ReferredBy);
                return false;
            }

            _logger.LogInformation("All validations passed. Creating referral record...");

            // Create referral
            var referral = new Referral
            {
                ReferrerId = referrer.Id,
                ReferredUserId = newUserId,
                CreatedAt = DateTime.UtcNow,
                CommissionEarned = 0,
                CommissionPaid = false,
                PaidAt = null,
                IsActive = true
            };

            // Update user's ReferredBy field
            newUser.ReferredBy = referrer.Id;
            
            // Add referral to context
            _db.Referrals.Add(referral);
            
            _logger.LogInformation("Saving referral data to database...");
            
            // Save changes
            var changesSaved = await _db.SaveChangesAsync().ConfigureAwait(false);
            
            _logger.LogInformation("Successfully processed referral: {ReferrerId} referred {NewUserId}. Changes saved: {ChangesSaved}", 
                referrer.Id, newUserId, changesSaved);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing referral for code '{ReferrerCode}' and user '{NewUserId}'", referrerCode, newUserId);
            return false;
        }
    }

    public async Task<decimal> CalculateCommissionAsync(string referrerId)
    {
        try
        {
            var propertyCommissions = await _db.Referrals
                .Where(r => r.ReferrerId == referrerId && r.IsActive && !r.CommissionPaid)
                .Join(_db.Properties.Where(p => p.Status == PropertyStatus.Approved),
                    r => r.ReferredUserId,
                    p => p.UserId,
                    (r, p) => new { r, p })
                .ToListAsync()
                .ConfigureAwait(false);

            decimal totalCommission = 0;
            foreach (var item in propertyCommissions)
            {
                var propertyPrice = item.p.Price;
                var propertyCurrency = item.p.CurrencyCode ?? "USD";
                
                // Convert property price to USD if needed
                if (propertyCurrency != "USD")
                {
                    propertyPrice = await _currencyService.ConvertAmountAsync(propertyPrice, propertyCurrency, "USD");
                }
                
                totalCommission += propertyPrice * _commissionPercent;
            }
            
            _logger.LogInformation("Calculated commission for referrer {ReferrerId}: ${Commission} USD", referrerId, totalCommission);
            return totalCommission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating commission for referrer {ReferrerId}", referrerId);
            return 0m;
        }
    }

    public async Task<IEnumerable<Referral>> GetUserReferralsAsync(string userId)
    {
        try
        {
            var referrals = await _db.Referrals
                .AsNoTracking()
                .Where(r => r.ReferrerId == userId && r.IsActive)
                .Include(r => r.ReferredUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync()
                .ConfigureAwait(false);
            _logger.LogInformation("Fetched {Count} referrals for user {UserId}", referrals.Count, userId);
            return referrals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching referrals for user {UserId}", userId);
            return Enumerable.Empty<Referral>();
        }
    }

    public async Task<string> GenerateReferralCodeAsync(string userId)
    {
        try
        {
            string code;
            do
            {
                code = GenerateRandomCode(ReferralCodeLength);
            } while (await _db.Users.AnyAsync(u => u.ReferralCode == code).ConfigureAwait(false));

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for referral code generation", userId);
                return string.Empty;
            }
            user.ReferralCode = code;
            await _db.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Generated referral code {Code} for user {UserId}", code, userId);
            return code;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating referral code for user {UserId}", userId);
            return string.Empty;
        }
    }

    public async Task<ReferralStats> GetReferralStatsAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Getting referral stats for user {UserId}", userId);

            // Get user for referral code
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);
            
            // Generate referral code if user doesn't have one
            string? referralCode = user?.ReferralCode;
            if (user != null && string.IsNullOrEmpty(referralCode))
            {
                _logger.LogInformation("User {UserId} doesn't have a referral code. Generating one.", userId);
                referralCode = await GenerateReferralCodeAsync(userId);
            }

            // Get all referral counts
            var totalReferrals = await _db.Referrals.CountAsync(r => r.ReferrerId == userId).ConfigureAwait(false);
            var activeReferrals = await _db.Referrals.CountAsync(r => r.ReferrerId == userId && r.IsActive).ConfigureAwait(false);
            
            _logger.LogInformation("User {UserId} has {TotalReferrals} total referrals, {ActiveReferrals} active", 
                userId, totalReferrals, activeReferrals);

            // Get commission data with better error handling
            var allCommissions = await _db.Referrals
                .Where(r => r.ReferrerId == userId && r.IsActive)
                .Join(_db.Properties.Where(p => p.Status == PropertyStatus.Approved),
                    r => r.ReferredUserId,
                    p => p.UserId,
                    (r, p) => new { r, p })
                .ToListAsync().ConfigureAwait(false);

            _logger.LogInformation("Found {CommissionCount} commission-eligible properties for user {UserId}", 
                allCommissions.Count, userId);

            // Calculate total commission with improved error handling
            decimal totalCommission = 0;
            decimal pendingCommission = 0;
            decimal commissionPaid = 0;
            int conversionErrors = 0;

            foreach (var item in allCommissions)
            {
                try
                {
                    var propertyPrice = item.p.Price;
                    var propertyCurrency = item.p.CurrencyCode ?? "USD";
                    
                    // Convert property price to USD if needed
                    if (propertyCurrency != "USD")
                    {
                        try
                        {
                            propertyPrice = await _currencyService.ConvertAmountAsync(propertyPrice, propertyCurrency, "USD");
                        }
                        catch (Exception conversionEx)
                        {
                            _logger.LogWarning(conversionEx, "Failed to convert {Currency} to USD for property {PropertyId}, using original price", 
                                propertyCurrency, item.p.Id);
                            conversionErrors++;
                            // Use original price if conversion fails
                        }
                    }
                    
                    var commission = propertyPrice * _commissionPercent;
                    totalCommission += commission;

                    if (!item.r.CommissionPaid)
                    {
                        pendingCommission += commission;
                    }
                    else
                    {
                        commissionPaid += commission;
                    }
                }
                catch (Exception itemEx)
                {
                    _logger.LogWarning(itemEx, "Error processing commission for referral {ReferralId}, property {PropertyId}", 
                        item.r.Id, item.p.Id);
                }
            }

            if (conversionErrors > 0)
            {
                _logger.LogWarning("Encountered {ConversionErrors} currency conversion errors for user {UserId}", 
                    conversionErrors, userId);
            }

            var stats = new ReferralStats
            {
                TotalReferrals = totalReferrals,
                ActiveReferrals = activeReferrals,
                TotalCommissionEarned = totalCommission,
                PendingCommission = pendingCommission,
                CommissionPaid = commissionPaid,
                ReferralCode = referralCode
            };

            _logger.LogInformation("Calculated referral stats for user {UserId}: Total={TotalReferrals}, Active={ActiveReferrals}, Commission=${TotalCommission}", 
                userId, totalReferrals, activeReferrals, totalCommission);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting referral stats for user {UserId}", userId);
            return new ReferralStats
            {
                TotalReferrals = 0,
                ActiveReferrals = 0,
                TotalCommissionEarned = 0,
                PendingCommission = 0,
                CommissionPaid = 0,
                ReferralCode = null
            };
        }
    }

    public async Task<bool> ProcessCommissionPaymentAsync(int referralId)
    {
        try
        {
            var referral = await _db.Referrals.FirstOrDefaultAsync(r => r.Id == referralId).ConfigureAwait(false);
            if (referral == null || referral.CommissionPaid)
            {
                _logger.LogWarning("Referral {ReferralId} not found or already paid", referralId);
                return false;
            }
            referral.CommissionPaid = true;
            referral.PaidAt = DateTime.UtcNow;
            await _db.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Processed commission payment for referral {ReferralId}", referralId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing commission payment for referral {ReferralId}", referralId);
            return false;
        }
    }

    public async Task<bool> ValidateReferralCodeAsync(string referralCode)
    {
        try
        {
            var exists = await _db.Users.AnyAsync(u => u.ReferralCode == referralCode).ConfigureAwait(false);
            _logger.LogInformation("Referral code {ReferralCode} validation result: {Exists}", referralCode, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating referral code {ReferralCode}", referralCode);
            return false;
        }
    }

    public async Task<User?> GetUserByReferralCodeAsync(string referralCode)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.ReferralCode == referralCode).ConfigureAwait(false);
            if (user != null)
                _logger.LogInformation("Found user {UserId} for referral code {ReferralCode}", user.Id, referralCode);
            else
                _logger.LogInformation("No user found for referral code {ReferralCode}", referralCode);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by referral code {ReferralCode}", referralCode);
            return null;
        }
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var result = new StringBuilder(length);
        foreach (var b in bytes)
            result.Append(chars[b % chars.Length]);
        return result.ToString();
    }
}
