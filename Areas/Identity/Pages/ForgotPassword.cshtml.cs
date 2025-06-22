using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Identity.Pages;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly INotificationService _notificationService;

    public ForgotPasswordModel(UserManager<User> userManager, INotificationService notificationService)
    {
        _userManager = userManager;
        _notificationService = notificationService;
    }

    [BindProperty]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string? ResultMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            ResultMessage = "If an account exists for this email, a reset link has been sent.";
            return Page();
        }
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = Url.Page("/Identity/ResetPassword", null, new { email = Email, token }, Request.Scheme);
        await _notificationService.SendEmailAsync(Email, "Reset your password", $"Reset your password using this link: <a href='{resetUrl}'>Reset Password</a>", true);
        ResultMessage = "If an account exists for this email, a reset link has been sent.";
        return Page();
    }
}
