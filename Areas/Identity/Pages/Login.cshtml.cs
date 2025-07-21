using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Identity.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IUserService _userService;

    public LoginModel(SignInManager<User> signInManager, ILogger<LoginModel> logger, IUserService userService)
    {
        _signInManager = signInManager;
        _logger = logger;
        _userService = userService;
    }

    [BindProperty]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [BindProperty]
    public bool RememberMe { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
            return Page();
        var result = await _signInManager.PasswordSignInAsync(Email, Password, RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in.", Email);
            // TODO: Track login activity
            if (string.IsNullOrEmpty(ReturnUrl) || ReturnUrl == "~/")
            {
                return RedirectToPage("/Dashboard/Index", new { area = "Membership" });
            }
            return LocalRedirect(ReturnUrl);
        }
        if (result.IsLockedOut)
        {
            ErrorMessage = "Account locked out. Please try again later.";
            return Page();
        }
        if (result.RequiresTwoFactor)
        {
            // TODO: Handle 2FA
            ErrorMessage = "Two-factor authentication required.";
            return Page();
        }
        ErrorMessage = "Invalid login attempt.";
        return Page();
    }
}
