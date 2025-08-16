using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Academy.UpgradeRequests
{
    [Authorize(Roles = "Admin")]
    public class PendingModel : PageModel
    {
        private readonly IUpgradeRequestService _upgradeRequestService;
        private readonly UserManager<User> _userManager;

        public PendingModel(IUpgradeRequestService upgradeRequestService, UserManager<User> userManager)
        {
            _upgradeRequestService = upgradeRequestService;
            _userManager = userManager;
        }

        public IList<UpgradeRequest> PendingRequests { get; set; } = new List<UpgradeRequest>();

        public async Task OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> 
            { 
                ("Academy", "/Admin/Academy"), 
                ("Upgrade Requests", "/Admin/Academy/UpgradeRequests/Index"),
                ("Pending Requests", "/Admin/Academy/UpgradeRequests/Pending")
            };

            PendingRequests = await _upgradeRequestService.GetPendingUpgradeRequestsAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int requestId)
        {
            var adminUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized();
            }

            var result = await _upgradeRequestService.ApproveUpgradeRequestAsync(requestId, adminUserId);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Upgrade request approved successfully. User has been upgraded to Premium Academy Package.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve upgrade request. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int requestId, string? rejectionReason)
        {
            var adminUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized();
            }

            var result = await _upgradeRequestService.RejectUpgradeRequestAsync(requestId, adminUserId, rejectionReason);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Upgrade request rejected successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject upgrade request. Please try again.";
            }

            return RedirectToPage();
        }
    }
}