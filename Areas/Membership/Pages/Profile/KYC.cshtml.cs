using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;
using SteadyGrowth.Web.Services.Interfaces;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Profile
{
    public class KYCModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;

        public KYCModel(UserManager<User> userManager, IUserService userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        public User AppUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Identity/Login");
            }

            AppUser = await _userService.GetUserByIdAsync(userId);
            if (AppUser == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile document)
        {
            if (document != null && document.Length > 0)
            {
                var user = await _userManager.GetUserAsync(User);
                user.KYCStatus = KYCStatus.Submitted;
                await _userManager.UpdateAsync(user);

                // In a real application, you would save the document to a secure location.
                // For this example, we'll just update the status.

                return RedirectToPage();
            }

            return Page();
        }
    }
}
