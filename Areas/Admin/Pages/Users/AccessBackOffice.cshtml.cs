using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class AccessBackOfficeModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AccessBackOfficeModel> _logger;

        public AccessBackOfficeModel(
            UserManager<User> userManager,
            ICurrentUserService currentUserService,
            ILogger<AccessBackOfficeModel> logger)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required");
            }

            // Get the user to impersonate
            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser == null)
            {
                return NotFound("User not found");
            }

            // Prevent impersonating admin users
            if (await _userManager.IsInRoleAsync(targetUser, "Admin"))
            {
                return BadRequest("Cannot impersonate admin users");
            }

            // Get current admin user for logging
            var adminUserId = _currentUserService.GetUserId();
            var adminUser = await _userManager.FindByIdAsync(adminUserId);

            // Set session for impersonation
            HttpContext.Session.SetString("Admin_ImpersonatedUserId", id);
            HttpContext.Session.SetString("Admin_ActualUserId", adminUserId);

            // Log the impersonation activity
            _logger.LogInformation("Admin {AdminEmail} started impersonating user {TargetEmail} (ID: {TargetId})", 
                adminUser?.Email, targetUser.Email, targetUser.Id);

            // Redirect to membership dashboard
            return Redirect("/Membership");
        }
    }
}