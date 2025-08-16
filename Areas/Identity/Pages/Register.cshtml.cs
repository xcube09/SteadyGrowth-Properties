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

    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IReferralService _referralService;
        private readonly IRewardService _rewardService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RegisterModel(UserManager<User> userManager, SignInManager<User> signInManager, IReferralService referralService, IRewardService rewardService, ApplicationDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _referralService = referralService;
            _rewardService = rewardService;
            _context = context;
            _configuration = configuration;
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
        // Set reCAPTCHA site key for the view
        ViewData["RecaptchaSiteKey"] = _configuration["Recaptcha:SiteKey"];
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // This method is kept for backward compatibility
        // The actual registration is handled via AJAX through AccountApiController
        // If someone tries to post directly, redirect them to the page
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
