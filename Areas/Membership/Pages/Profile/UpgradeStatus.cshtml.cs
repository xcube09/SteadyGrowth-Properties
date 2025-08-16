using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Profile
{
    public class UpgradeStatusModel : PageModel
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;

        public UpgradeStatusModel(ICurrentUserService currentUserService, ApplicationDbContext context)
        {
            _currentUserService = currentUserService;
            _context = context;
        }

        public IList<UpgradeRequest> UpgradeRequests { get; set; } = new List<UpgradeRequest>();

        public async Task OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> 
            { 
                ("My Profile", "/Membership/Profile/Index"), 
                ("Upgrade Status", "/Membership/Profile/UpgradeStatus") 
            };

            var userId = _currentUserService.GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                UpgradeRequests = await _context.UpgradeRequests
                    .Include(ur => ur.RequestedPackage)
                    .Where(ur => ur.UserId == userId)
                    .OrderByDescending(ur => ur.RequestedAt)
                    .ToListAsync();
            }
        }
    }
}