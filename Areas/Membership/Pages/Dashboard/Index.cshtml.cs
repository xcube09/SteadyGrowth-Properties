using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Models.Enums;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

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
    private readonly UserManager<Models.Entities.User> _userManager;

	public IndexModel(IUserService userService, IPropertyService propertyService, IReferralService referralService, IRewardService rewardService, UserManager<Models.Entities.User> userManager)
	{
		_userService = userService;
		_propertyService = propertyService;
		_referralService = referralService;
		_rewardService = rewardService;
		_userManager = userManager;
	}

	public UserStats? UserStats { get; set; }
    public ReferralStats? ReferralStats { get; set; }
    public int RewardPoints { get; set; }
    public KYCStatus KYCStatus { get; set; }
    public IList<Models.Entities.Property> RecentProperties { get; set; } = new List<Models.Entities.Property>();

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return;
        UserStats = await _userService.GetUserStatsAsync(user.Id);
        ReferralStats = await _referralService.GetReferralStatsAsync(user.Id);
        RewardPoints = await _rewardService.GetUserTotalPointsAsync(user.Id);
        RecentProperties = (await _propertyService.GetUserPropertiesAsync(user.Id)).Take(5).ToList();
        //var user = await _userService.GetUserByIdAsync(user.Id);
        if (user != null) KYCStatus = user.KYCStatus;
    }

    
}
