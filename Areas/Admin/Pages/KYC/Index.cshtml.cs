using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Admin.Pages.KYC
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public IndexModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public List<User> Users { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _userManager.Users.Where(u => u.KYCStatus != KYCStatus.NotStarted).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string userId, string action)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (action == "Approve")
                {
                    user.KYCStatus = KYCStatus.Approved;
                }
                else if (action == "Reject")
                {
                    user.KYCStatus = KYCStatus.Rejected;
                }

                await _userManager.UpdateAsync(user);
            }

            return RedirectToPage();
        }
    }
}
