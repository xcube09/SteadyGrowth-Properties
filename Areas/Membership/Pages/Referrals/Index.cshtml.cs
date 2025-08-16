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
    private readonly ICurrentUserService _currentUserService;

    public IndexModel(IReferralService referralService, ICurrentUserService currentUserService)
    {
        _referralService = referralService;
        _currentUserService = currentUserService;
    }

    public string? ReferralCode { get; set; }
    public string? ReferralLink { get; set; }
    public ReferralStats? Stats { get; set; }
    public IList<SteadyGrowth.Web.Models.Entities.Referral> Referrals { get; set; } = new List<SteadyGrowth.Web.Models.Entities.Referral>();

    public async Task OnGetAsync()
    {
        var userId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return;
        Stats = await _referralService.GetReferralStatsAsync(userId);
        ReferralCode = Stats?.ReferralCode;
        Referrals = (await _referralService.GetUserReferralsAsync(userId)).ToList();
        
        // Generate referral link using the same format as the navigation modal
        if (!string.IsNullOrEmpty(ReferralCode))
        {
            var scheme = Request.Scheme;
            var host = Request.Host.ToString();
            ReferralLink = $"{scheme}://{host}/Identity/Register?referrerId={ReferralCode}";
        }
    }
}
