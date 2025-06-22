using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Profile;

/// <summary>
/// User profile edit page model for members.
/// </summary>
[Authorize]
public class EditModel : PageModel
{
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;

    public EditModel(IUserService userService, UserManager<User> userManager)
    {
        _userService = userService;
        _userManager = userManager;
    }

    [BindProperty]
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [BindProperty]
    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    [BindProperty]
    [EmailAddress]
    public string? Email { get; set; }
    [BindProperty]
    [StringLength(100)]
    public string? CurrentPassword { get; set; }
    [BindProperty]
    [StringLength(100)]
    public string? NewPassword { get; set; }
    [BindProperty]
    [StringLength(100)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }

    [TempData]
    public string? ResultMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();
        FirstName = user.FirstName;
        LastName = user.LastName;
        Email = user.Email;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();
        user.FirstName = FirstName;
        user.LastName = LastName;
        if (!string.IsNullOrEmpty(Email) && Email != user.Email)
        {
            user.Email = Email;
            user.UserName = Email;
        }
        await _userService.UpdateUserAsync(user);
        // Handle password change
        if (!string.IsNullOrEmpty(CurrentPassword) && !string.IsNullOrEmpty(NewPassword))
        {
            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }
        }
        ResultMessage = "Profile updated successfully.";
        return RedirectToPage("Edit");
    }
}
