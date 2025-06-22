using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Dashboard;

/// <summary>
/// Member dashboard page model.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IPropertyService _propertyService;
    private readonly IReferralService _referralService;
    private readonly IRewardService _rewardService;

    public IndexModel(IUserService userService, IPropertyService propertyService, IReferralService referralService, IRewardService rewardService)
    {
        _userService = userService;
        _propertyService = propertyService;
        _referralService = referralService;
        _rewardService = rewardService;
    }

    public UserStats? UserStats { get; set; }
    public ReferralStats? ReferralStats { get; set; }
    public int RewardPoints { get; set; }
    public IList<SteadyGrowth.Web.Models.Entities.Property> RecentProperties { get; set; } = new List<SteadyGrowth.Web.Models.Entities.Property>();

    public async Task OnGetAsync()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId)) return;
        UserStats = await _userService.GetUserStatsAsync(userId);
        ReferralStats = await _referralService.GetReferralStatsAsync(userId);
        RewardPoints = await _rewardService.GetUserTotalPointsAsync(userId);
        RecentProperties = (await _propertyService.GetUserPropertiesAsync(userId)).Take(5).ToList();
    }
}
