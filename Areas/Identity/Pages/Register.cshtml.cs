using Griesoft.AspNetCore.ReCaptcha;
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

namespace SteadyGrowth.Web.Areas.Identity.Pages;

    [ValidateRecaptcha(Action = "register")]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IReferralService _referralService;
        private readonly IRewardService _rewardService;
        private readonly ApplicationDbContext _context;

        public RegisterModel(UserManager<User> userManager, SignInManager<User> signInManager, IReferralService referralService, IRewardService rewardService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _referralService = referralService;
            _rewardService = rewardService;
            _context = context;
        }

    [BindProperty]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [BindProperty]
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [BindProperty]
    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    [BindProperty]
    [Phone]
    public string? PhoneNumber { get; set; }
    [BindProperty]
    public string? ReferralCode { get; set; }
    [BindProperty]
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [BindProperty]
    [Required, DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void OnGet(string? referrerId = null)
    {
        ReferralCode = referrerId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

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
        } while (await _userManager.FindByNameAsync(generatedReferralCode) != null); // Check for collision

        user.ReferralCode = generatedReferralCode;

        var result = await _userManager.CreateAsync(user, Password);
        if (result.Succeeded)
        {
            // Assign Basic Package by default
            var basicPackage = await _context.AcademyPackages.FirstOrDefaultAsync(p => p.Name == "Basic Package");
            if (basicPackage != null)
            {
                user.AcademyPackageId = basicPackage.Id;
                await _userManager.UpdateAsync(user);
            }

            if (!string.IsNullOrWhiteSpace(ReferralCode))
                await _referralService.ProcessReferralAsync(ReferralCode, user.Id);
            // TODO: Send welcome email and referral rewards
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToPage("/Membership/Dashboard/Index");
        }
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        return Page();
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
