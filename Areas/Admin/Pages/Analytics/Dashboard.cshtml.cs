using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Analytics;

/// <summary>
/// Admin analytics dashboard page model.
/// </summary>
[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IPropertyService _propertyService;
    private readonly IReferralService _referralService;
    private readonly IRewardService _rewardService;

    public DashboardModel(IUserService userService, IPropertyService propertyService, IReferralService referralService, IRewardService rewardService)
    {
        _userService = userService;
        _propertyService = propertyService;
        _referralService = referralService;
        _rewardService = rewardService;
    }

    public int TotalUsers { get; set; }
    public int TotalProperties { get; set; }
    public int TotalReferrals { get; set; }
    public int TotalRewardPoints { get; set; }
    // Add more analytics properties as needed

    public async Task OnGetAsync()
    {
        TotalUsers = await _userService.GetTotalUsersCountAsync();
        TotalProperties = (await _propertyService.GetApprovedPropertiesAsync(1, 1_000_000)).Count();
        TotalReferrals = (await _referralService.GetUserReferralsAsync("")).Count(); // Replace with actual logic
        TotalRewardPoints = 0; // Replace with actual logic
        // TODO: Add chart data, KPIs, and audit logging
    }
}
