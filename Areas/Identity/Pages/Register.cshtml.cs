using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using SteadyGrowth.Web.Data;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Areas.Identity.Pages;

public class RegisterModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IPasswordValidator<User> _passwordValidator;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<User> userManager,
        IPasswordValidator<User> passwordValidator,
        IPasswordHasher<User> passwordHasher,
        ApplicationDbContext context,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _passwordValidator = passwordValidator;
        _passwordHasher = passwordHasher;
        _context = context;
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

    [BindProperty]
    [Required(ErrorMessage = "Please select a package")]
    [Display(Name = "Selected Package")]
    public int SelectedPackageId { get; set; }

    public IList<AcademyPackage> AvailablePackages { get; set; } = new List<AcademyPackage>();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? referrerId = null)
    {
        ReferralCode = referrerId;
        // Load available packages
        AvailablePackages = await _context.AcademyPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Load packages for re-display if validation fails
            AvailablePackages = await _context.AcademyPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync();

            // Check if ModelState is valid
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if email already exists in Users table
            var existingUser = await _userManager.FindByEmailAsync(Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return Page();
            }

            // Check if email already exists in PendingRegistrations table
            var pendingExists = await _context.PendingRegistrations
                .AnyAsync(p => p.Email == Email && !p.IsDeleted && p.Status == PendingRegistrationStatus.Pending);
            if (pendingExists)
            {
                ModelState.AddModelError("Email", "A registration with this email is already pending approval.");
                return Page();
            }

            // Validate password using Identity password validator
            var tempUser = new User { UserName = Email, Email = Email };
            var passwordResult = await _passwordValidator.ValidateAsync(_userManager, tempUser, Password);
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError("Password", error.Description);
                }
                return Page();
            }

            // Hash the password for storage
            var hashedPassword = _passwordHasher.HashPassword(tempUser, Password);

            // Validate referral code if provided
            if (!string.IsNullOrWhiteSpace(ReferralCode))
            {
                var referrerExists = await _context.Users.AnyAsync(u => u.ReferralCode == ReferralCode);
                if (!referrerExists)
                {
                    ModelState.AddModelError("ReferralCode", "Invalid referral code.");
                    return Page();
                }
            }

            // Create pending registration
            var pendingRegistration = new PendingRegistration
            {
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                PhoneNumber = PhoneNumber,
                PasswordHash = hashedPassword,
                SelectedPackageId = SelectedPackageId,
                ReferralCode = ReferralCode,
                Status = PendingRegistrationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.PendingRegistrations.Add(pendingRegistration);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Pending registration created for {Email} with package {PackageId}",
                Email, SelectedPackageId);

            // Redirect to payment instructions page
            return RedirectToPage("/RegistrationPending", new { email = Email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email {Email}", Email);
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            return Page();
        }
    }
}
