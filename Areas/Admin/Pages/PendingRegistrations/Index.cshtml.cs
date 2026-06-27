using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;
using SteadyGrowth.Web.Services.Interfaces;
using System.Text.Json;

namespace SteadyGrowth.Web.Areas.Admin.Pages.PendingRegistrations;

[Authorize(Roles = "Admin")]
[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IReferralService _referralService;
    private readonly IReferralCommissionService _referralCommissionService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        ApplicationDbContext context,
        UserManager<User> userManager,
        IReferralService referralService,
        IReferralCommissionService referralCommissionService,
        ILogger<IndexModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _referralService = referralService;
        _referralCommissionService = referralCommissionService;
        _logger = logger;
    }

    public List<PendingRegistrationViewModel> Registrations { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = "Pending";
    public int TotalCount { get; set; }

    public async Task OnGetAsync(string? search = null, string? status = "Pending")
    {
        ViewData["Breadcrumb"] = new List<(string, string)> { ("Admin", "/Admin/Index"), ("Pending Registrations", "") };

        SearchTerm = search ?? string.Empty;
        StatusFilter = status ?? "Pending";

        var query = _context.PendingRegistrations
            .Include(p => p.SelectedPackage)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Apply status filter
        if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<PendingRegistrationStatus>(StatusFilter, out var statusEnum))
        {
            query = query.Where(p => p.Status == statusEnum);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var searchLower = SearchTerm.ToLower();
            query = query.Where(p =>
                p.Email.ToLower().Contains(searchLower) ||
                p.FirstName.ToLower().Contains(searchLower) ||
                p.LastName.ToLower().Contains(searchLower));
        }

        var registrations = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        TotalCount = registrations.Count;

        Registrations = registrations.Select(p => new PendingRegistrationViewModel
        {
            Id = p.Id,
            FullName = $"{p.FirstName} {p.LastName}",
            Email = p.Email,
            PhoneNumber = p.PhoneNumber,
            PackageName = p.SelectedPackage?.Name ?? "Unknown",
            PackagePrice = p.SelectedPackage?.Price ?? 0,
            ReferralCode = p.ReferralCode,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            ProcessedAt = p.ProcessedAt,
            Notes = p.Notes
        }).ToList();
    }

    public async Task<IActionResult> OnPostApproveAsync()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var data = JsonDocument.Parse(body).RootElement;
            var registrationId = data.GetProperty("id").GetInt32();

            var pendingReg = await _context.PendingRegistrations
                .Include(p => p.SelectedPackage)
                .FirstOrDefaultAsync(p => p.Id == registrationId && !p.IsDeleted);

            if (pendingReg == null)
            {
                return new JsonResult(new { success = false, message = "Registration not found." });
            }

            if (pendingReg.Status != PendingRegistrationStatus.Pending)
            {
                return new JsonResult(new { success = false, message = "Registration has already been processed." });
            }

            // Check if email is already taken
            var existingUser = await _userManager.FindByEmailAsync(pendingReg.Email);
            if (existingUser != null)
            {
                return new JsonResult(new { success = false, message = "Email is already registered to another user." });
            }

            // Create the user
            var user = new User
            {
                UserName = pendingReg.Email,
                Email = pendingReg.Email,
                FirstName = pendingReg.FirstName,
                LastName = pendingReg.LastName,
                PhoneNumber = pendingReg.PhoneNumber,
                AcademyPackageId = pendingReg.SelectedPackageId,
                EmailConfirmed = true
            };

            // Generate unique referral code
            string generatedReferralCode;
            do
            {
                generatedReferralCode = GenerateReferralCode();
            } while (await _context.Users.AnyAsync(u => u.ReferralCode == generatedReferralCode));

            user.ReferralCode = generatedReferralCode;

            // Create user with the stored password hash
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user for pending registration {Id}: {Errors}", registrationId, errors);
                return new JsonResult(new { success = false, message = $"Failed to create user: {errors}" });
            }

            // Set the password hash directly (it was already hashed during registration)
            user.PasswordHash = pendingReg.PasswordHash;
            await _userManager.UpdateAsync(user);

            // Process referral if code was provided
            if (!string.IsNullOrWhiteSpace(pendingReg.ReferralCode))
            {
                _logger.LogInformation("Processing referral for new user {UserId} with code {ReferralCode}",
                    user.Id, pendingReg.ReferralCode);

                var referralProcessed = await _referralService.ProcessReferralAsync(pendingReg.ReferralCode, user.Id);
                if (referralProcessed)
                {
                    _logger.LogInformation("Referral processed successfully for user {UserId}", user.Id);

                    // Process referral commissions
                    await _referralCommissionService.ProcessPackagePurchaseCommissionsAsync(user.Id, pendingReg.SelectedPackageId);
                    _logger.LogInformation("Referral commissions processed for user {UserId}", user.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to process referral for user {UserId} with code {ReferralCode}",
                        user.Id, pendingReg.ReferralCode);
                }
            }

            // Create wallet for the user
            var wallet = new Wallet
            {
                UserId = user.Id,
                Balance = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Wallets.Add(wallet);

            // Update pending registration status
            pendingReg.Status = PendingRegistrationStatus.Approved;
            pendingReg.ProcessedAt = DateTime.UtcNow;
            pendingReg.ProcessedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            pendingReg.CreatedUserId = user.Id;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pending registration {Id} approved. User {UserId} created with package {PackageId}",
                registrationId, user.Id, pendingReg.SelectedPackageId);

            return new JsonResult(new { success = true, message = "Registration approved and user account created." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving pending registration");
            return new JsonResult(new { success = false, message = "An error occurred: " + ex.Message });
        }
    }

    public async Task<IActionResult> OnPostRejectAsync()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var data = JsonDocument.Parse(body).RootElement;
            var registrationId = data.GetProperty("id").GetInt32();
            var reason = data.TryGetProperty("reason", out var reasonProp) ? reasonProp.GetString() : null;

            var pendingReg = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Id == registrationId && !p.IsDeleted);

            if (pendingReg == null)
            {
                return new JsonResult(new { success = false, message = "Registration not found." });
            }

            if (pendingReg.Status != PendingRegistrationStatus.Pending)
            {
                return new JsonResult(new { success = false, message = "Registration has already been processed." });
            }

            // Update status to rejected
            pendingReg.Status = PendingRegistrationStatus.Rejected;
            pendingReg.ProcessedAt = DateTime.UtcNow;
            pendingReg.ProcessedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            pendingReg.Notes = reason;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pending registration {Id} rejected. Reason: {Reason}", registrationId, reason);

            return new JsonResult(new { success = true, message = "Registration rejected." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting pending registration");
            return new JsonResult(new { success = false, message = "An error occurred: " + ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var data = JsonDocument.Parse(body).RootElement;
            var registrationId = data.GetProperty("id").GetInt32();

            var pendingReg = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Id == registrationId);

            if (pendingReg == null)
            {
                return new JsonResult(new { success = false, message = "Registration not found." });
            }

            // Soft delete
            pendingReg.IsDeleted = true;
            pendingReg.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pending registration {Id} soft deleted", registrationId);

            return new JsonResult(new { success = true, message = "Registration deleted." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting pending registration");
            return new JsonResult(new { success = false, message = "An error occurred: " + ex.Message });
        }
    }

    private string GenerateReferralCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public class PendingRegistrationViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public decimal PackagePrice { get; set; }
        public string? ReferralCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Notes { get; set; }
    }
}
