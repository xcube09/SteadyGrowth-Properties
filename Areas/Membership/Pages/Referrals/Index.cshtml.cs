using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Referrals;

/// <summary>
/// Referral management page model for members.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IReferralService _referralService;

    public IndexModel(IReferralService referralService)
    {
        _referralService = referralService;
    }

    public string? ReferralCode { get; set; }
    public ReferralStats? Stats { get; set; }
    public IList<SteadyGrowth.Web.Models.Entities.Referral> Referrals { get; set; } = new List<SteadyGrowth.Web.Models.Entities.Referral>();

    public async Task OnGetAsync()
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId)) return;
        Stats = await _referralService.GetReferralStatsAsync(userId);
        ReferralCode = Stats?.ReferralCode;
        Referrals = (await _referralService.GetUserReferralsAsync(userId)).ToList();
    }
}
