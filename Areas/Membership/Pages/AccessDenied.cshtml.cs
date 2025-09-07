using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Areas.Membership.Pages
{
    public class AccessDeniedModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public AccessDeniedModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public bool IsKYCDenial { get; set; } = false;
        public string? ReturnUrl { get; set; }

        public async Task OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            
            // For any authenticated user with non-approved KYC status, assume it's KYC-related
            // (since they wouldn't hit this page unless they tried to access restricted content)
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.KYCStatus != KYCStatus.Approved)
                {
                    IsKYCDenial = true;
                }
            }
        }
    }
}
