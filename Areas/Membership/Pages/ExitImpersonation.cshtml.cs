using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Areas.Membership.Pages
{
    [Authorize]
    public class ExitImpersonationModel : PageModel
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ExitImpersonationModel> _logger;

        public ExitImpersonationModel(
            ICurrentUserService currentUserService,
            UserManager<User> userManager,
            ILogger<ExitImpersonationModel> logger)
        {
            _currentUserService = currentUserService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_currentUserService.IsImpersonating())
            {
                return RedirectToPage("/Dashboard/Index", new { area = "Membership" });
            }

            // Get impersonated user info for logging
            var impersonatedUser = await _currentUserService.GetImpersonatedUserAsync();
            var adminUserId = _currentUserService.GetActualAdminUserId();
            var adminUser = await _userManager.FindByIdAsync(adminUserId ?? "");

            // Log the end of impersonation
            _logger.LogInformation("Admin {AdminEmail} stopped impersonating user {TargetEmail} (ID: {TargetId})", 
                adminUser?.Email, impersonatedUser?.Email, impersonatedUser?.Id);

            // Stop impersonation
            _currentUserService.StopImpersonation();

            return Page();
        }
    }
}