using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Identity.Pages;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public ResetPasswordModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [BindProperty]
    [Required]
    public string Token { get; set; } = string.Empty;
    [BindProperty]
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [BindProperty]
    [Required, DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? ResultMessage { get; set; }

    public void OnGet(string email, string token)
    {
        Email = email;
        Token = token;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            ResultMessage = "Invalid request.";
            return Page();
        }
        var result = await _userManager.ResetPasswordAsync(user, Token, Password);
        if (result.Succeeded)
        {
            ResultMessage = "Password reset successful. You may now log in.";
            return RedirectToPage("Login");
        }
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        return Page();
    }
}
