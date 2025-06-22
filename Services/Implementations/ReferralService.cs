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
    private readonly decimal _commissionPercent;
    private const int MaxReferralDepth = 3;
    private const int ReferralCodeLength = 8;

    public ReferralService(ApplicationDbContext db, ILogger<ReferralService> logger, IConfiguration config)
    {
        _db = db;
        _logger = logger;
        _config = config;
        _commissionPercent = _config.GetValue<decimal>("Referral:CommissionPercent", 0.05m);
    }

    public async Task<bool> ProcessReferralAsync(string referrerCode, string newUserId)
    {
        try
        {
            var referrer = await _db.Users.FirstOrDefaultAsync(u => u.ReferralCode == referrerCode).ConfigureAwait(false);
            var newUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == newUserId).ConfigureAwait(false);
            if (referrer == null || newUser == null)
            {
                _logger.LogWarning("Invalid referral: referrer or new user not found");
                return false;
            }
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
                _logger.LogWarning("User {UserId} already has a referrer", newUserId);
                return false;
            }
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
            newUser.ReferredBy = referrer.Id;
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Processed referral: {ReferrerId} referred {NewUserId}", referrer.Id, newUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing referral for code {ReferrerCode} and user {NewUserId}", referrerCode, newUserId);
            return false;
        }
    }

    public async Task<decimal> CalculateCommissionAsync(string referrerId)
    {
        try
        {
            var commission = await _db.Referrals
                .Where(r => r.ReferrerId == referrerId && r.IsActive && !r.CommissionPaid)
                .Join(_db.Properties.Where(p => p.Status == PropertyStatus.Approved),
                    r => r.ReferredUserId,
                    p => p.UserId,
                    (r, p) => new { r, p })
                .SumAsync(x => x.p.Price * _commissionPercent)
                .ConfigureAwait(false);
            _logger.LogInformation("Calculated commission for referrer {ReferrerId}: {Commission}", referrerId, commission);
            return commission;
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
            var totalReferrals = await _db.Referrals.CountAsync(r => r.ReferrerId == userId).ConfigureAwait(false);
            var activeReferrals = await _db.Referrals.CountAsync(r => r.ReferrerId == userId && r.IsActive).ConfigureAwait(false);
            var totalCommission = await _db.Referrals
                .Where(r => r.ReferrerId == userId && r.IsActive)
                .Join(_db.Properties.Where(p => p.Status == PropertyStatus.Approved),
                    r => r.ReferredUserId,
                    p => p.UserId,
                    (r, p) => p.Price * _commissionPercent)
                .SumAsync().ConfigureAwait(false);
            var pendingCommission = await _db.Referrals
                .Where(r => r.ReferrerId == userId && r.IsActive && !r.CommissionPaid)
                .Join(_db.Properties.Where(p => p.Status == PropertyStatus.Approved),
                    r => r.ReferredUserId,
                    p => p.UserId,
                    (r, p) => p.Price * _commissionPercent)
                .SumAsync().ConfigureAwait(false);
            var commissionPaid = await _db.Referrals
                .Where(r => r.ReferrerId == userId && r.IsActive && r.CommissionPaid)
                .Join(_db.Properties.Where(p => p.Status == PropertyStatus.Approved),
                    r => r.ReferredUserId,
                    p => p.UserId,
                    (r, p) => p.Price * _commissionPercent)
                .SumAsync().ConfigureAwait(false);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);
            return new ReferralStats
            {
                TotalReferrals = totalReferrals,
                ActiveReferrals = activeReferrals,
                TotalCommissionEarned = totalCommission,
                PendingCommission = pendingCommission,
                CommissionPaid = commissionPaid,
                ReferralCode = user?.ReferralCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting referral stats for user {UserId}", userId);
            return new ReferralStats();
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
