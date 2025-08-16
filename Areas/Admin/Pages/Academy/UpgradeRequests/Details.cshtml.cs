using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Academy.UpgradeRequests
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IUpgradeRequestService _upgradeRequestService;
        private readonly UserManager<User> _userManager;

        public DetailsModel(IUpgradeRequestService upgradeRequestService, UserManager<User> userManager)
        {
            _upgradeRequestService = upgradeRequestService;
            _userManager = userManager;
        }

        public UpgradeRequest? Request { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Request = await _upgradeRequestService.GetUpgradeRequestByIdAsync(id);
            
            if (Request == null)
            {
                return NotFound();
            }

            ViewData["Breadcrumb"] = new List<(string, string)> 
            { 
                ("Academy", "/Admin/Academy"), 
                ("Upgrade Requests", "/Admin/Academy/UpgradeRequests/Index"),
                ("Request Details", $"/Admin/Academy/UpgradeRequests/Details/{id}")
            };

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var adminUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized();
            }

            var result = await _upgradeRequestService.ApproveUpgradeRequestAsync(id, adminUserId);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Upgrade request approved successfully. User has been upgraded to Premium Academy Package.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve upgrade request. Please try again.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string? rejectionReason)
        {
            var adminUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized();
            }

            var result = await _upgradeRequestService.RejectUpgradeRequestAsync(id, adminUserId, rejectionReason);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Upgrade request rejected successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject upgrade request. Please try again.";
            }

            return RedirectToPage(new { id });
        }
    }
}