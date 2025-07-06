using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SteadyGrowth.Web.Application.Commands.Users;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Users;

/// <summary>
/// User details page model for admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class DetailsModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IPropertyService _propertyService;
    private readonly IReferralService _referralService;
    private readonly IMediator _mediator;

    public DetailsModel(IUserService userService, IPropertyService propertyService, IReferralService referralService, IMediator mediator)
    {
        _userService = userService;
        _propertyService = propertyService;
        _referralService = referralService;
        _mediator = mediator;
    }

    public User? AppUser { get; set; }
    public UserStats? UserStats { get; set; }
    public IList<SteadyGrowth.Web.Models.Entities.Property> Properties { get; set; } = new List<SteadyGrowth.Web.Models.Entities.Property>();
    public IList<SteadyGrowth.Web.Models.Entities.Referral> Referrals { get; set; } = new List<SteadyGrowth.Web.Models.Entities.Referral>();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        ViewData["Breadcrumb"] = new List<(string, string)> { ("Admin", "/Admin/Properties/Index"), ("Users", "/Admin/Users/Index"), ("Details", $"/Admin/Users/Details/{id}") };
        AppUser = await _userService.GetUserByIdAsync(id);
        if (AppUser == null)
        {
            // Handle user not found
            return NotFound();
        }
        UserStats = await _userService.GetUserStatsAsync(id);
        Properties = (await _propertyService.GetUserPropertiesAsync(id))!?.ToList();
        Referrals = (await _referralService.GetUserReferralsAsync(id)).ToList();
        // TODO: Add audit logging for admin view
        return Page();
    }

    public async Task<IActionResult> OnPostUploadProfilePictureAsync(string userId, IFormFile profilePicture)
    {
        if (profilePicture == null || profilePicture.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select a file to upload.");
            return RedirectToPage(new { id = userId });
        }

        var command = new UpdateUserProfilePictureCommand
        {
            UserId = userId,
            ProfilePicture = profilePicture
        };

        var result = await _mediator.Send(command);

        if (result)
        {
            // Success
            return RedirectToPage(new { id = userId });
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Error uploading profile picture.");
            return RedirectToPage(new { id = userId });
        }
    }
}
