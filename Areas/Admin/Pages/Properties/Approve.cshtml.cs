using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Properties;

/// <summary>
/// Approve or reject property vetting for admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class ApproveModel : PageModel
{
    private readonly IVettingService _vettingService;
    private readonly INotificationService _notificationService;

    public ApproveModel(IVettingService vettingService, INotificationService notificationService)
    {
        _vettingService = vettingService;
        _notificationService = notificationService;
    }

    [BindProperty]
    public int PropertyId { get; set; }
    [BindProperty]
    [Required]
    public bool Approve { get; set; }
    [BindProperty]
    [StringLength(1000)]
    public string? Notes { get; set; }

    [TempData]
    public string? ResultMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var adminId = User.Identity?.Name;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();
        bool result;
        if (Approve)
            result = await _vettingService.ApprovePropertyAsync(PropertyId, adminId, Notes);
        else
            result = await _vettingService.RejectPropertyAsync(PropertyId, adminId, Notes ?? "");
        if (result)
        {
            await _notificationService.NotifyPropertyStatusChangeAsync(adminId, PropertyId, Approve ? SteadyGrowth.Web.Models.Entities.PropertyStatus.Approved : SteadyGrowth.Web.Models.Entities.PropertyStatus.Rejected);
            ResultMessage = Approve ? "Property approved successfully." : "Property rejected.";
        }
        else
        {
            ResultMessage = "Failed to process property approval/rejection.";
        }
        // TODO: Add audit logging for admin action
        return RedirectToPage("Index");
    }
}
