using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System;
using System.Linq;
using SteadyGrowth.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace SteadyGrowth.Web.Areas.Identity.Pages;

    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IReferralService _referralService;
        private readonly IRewardService _rewardService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(UserManager<User> userManager, SignInManager<User> signInManager, IReferralService referralService, IRewardService rewardService, ApplicationDbContext context, IConfiguration configuration, ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _referralService = referralService;
            _rewardService = rewardService;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

    [BindProperty]
    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [BindProperty]
    [Required, StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    [BindProperty]
    [Required, StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    [BindProperty]
    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    [BindProperty]
    [Display(Name = "Referral Code")]
    public string? ReferralCode { get; set; }
    [BindProperty]
    [Required, DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
    [BindProperty]
    [Required, DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    [BindProperty]
    [Required(ErrorMessage = "You must consent to data processing")]
    [Display(Name = "Data Processing Consent")]
    public bool Consent1 { get; set; }
    [BindProperty]
    [Required(ErrorMessage = "You must accept the privacy policy")]
    [Display(Name = "Privacy Policy Consent")]
    public bool Consent2 { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet(string? referrerId = null)
    {
        ReferralCode = referrerId;
        // reCAPTCHA disabled
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Check if ModelState is valid
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return Page();
            }

            // Create new user
            var user = new User
            {
                UserName = Email,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                PhoneNumber = PhoneNumber
            };

            // Generate unique referral code
            string generatedReferralCode;
            do
            {
                generatedReferralCode = GenerateReferralCode();
            } while (await _context.Users.AnyAsync(u => u.ReferralCode == generatedReferralCode));

            user.ReferralCode = generatedReferralCode;

            // Use transaction scope to coordinate UserManager and ApplicationDbContext operations
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Create user with password
                var result = await _userManager.CreateAsync(user, Password);

                if (result.Succeeded)
                {
                    // Assign Basic Package by default
                    var basicPackage = await _context.AcademyPackages
                        .FirstOrDefaultAsync(p => p.Name == "Basic Package");

                    if (basicPackage != null)
                    {
                        user.AcademyPackageId = basicPackage.Id;
                        await _userManager.UpdateAsync(user);
                    }

                    // Process referral if provided
                    _logger.LogInformation("Processing registration for user {UserId}. Referral code provided: '{ReferralCode}'",
                        user.Id, ReferralCode ?? "None");

                    if (!string.IsNullOrWhiteSpace(ReferralCode))
                    {
                        _logger.LogInformation("Attempting to process referral with code {ReferralCode} for new user {UserId}",
                            ReferralCode, user.Id);

                        // Verify user exists in ApplicationDbContext before processing referral
                        var userExistsInContext = await _context.Users.AnyAsync(u => u.Id == user.Id);
                        _logger.LogInformation("User {UserId} exists in ApplicationDbContext: {UserExists}", user.Id, userExistsInContext);

                        if (!userExistsInContext)
                        {
                            _logger.LogWarning("User {UserId} not found in ApplicationDbContext, attempting to refresh context", user.Id);

                            // Try to find the user after a brief delay
                            await Task.Delay(100);
                            userExistsInContext = await _context.Users.AnyAsync(u => u.Id == user.Id);
                            _logger.LogInformation("User {UserId} exists in ApplicationDbContext after delay: {UserExists}", user.Id, userExistsInContext);
                        }

                        // Verify referrer exists
                        var referrerExists = await _context.Users.AnyAsync(u => u.ReferralCode == ReferralCode);
                        _logger.LogInformation("Referrer with code {ReferralCode} exists in ApplicationDbContext: {ReferrerExists}",
                            ReferralCode, referrerExists);

                        if (!referrerExists)
                        {
                            _logger.LogWarning("Referrer with code {ReferralCode} not found in database", ReferralCode);
                        }

                        var referralProcessed = await _referralService.ProcessReferralAsync(ReferralCode, user.Id);
                        if (!referralProcessed)
                        {
                            _logger.LogWarning("Failed to process referral for user {UserId} with code {ReferralCode}. UserExists={UserExists}, ReferrerExists={ReferrerExists}",
                                user.Id, ReferralCode, userExistsInContext, referrerExists);
                            // Note: We continue with registration even if referral fails
                        }
                        else
                        {
                            _logger.LogInformation("Successfully processed referral for user {UserId} with code {ReferralCode}",
                                user.Id, ReferralCode);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No referral code provided for user {UserId}", user.Id);
                    }

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Complete the transaction
                    transactionScope.Complete();

                    _logger.LogInformation("User {Email} registered successfully", Email);

                    TempData["SuccessMessage"] = "Registration successful! Welcome to SteadyGrowth.";
                    return RedirectToPage("/Dashboard/Index", new { area = "Membership" });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email {Email}", Email);
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            return Page();
        }
    }

    private string GenerateReferralCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var result = new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        return result;
    }
}
