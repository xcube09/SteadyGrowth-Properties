using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Users;

/// <summary>
/// User details page model for admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class DetailsModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IPropertyService _propertyService;
    private readonly IReferralService _referralService;

    public DetailsModel(IUserService userService, IPropertyService propertyService, IReferralService referralService)
    {
        _userService = userService;
        _propertyService = propertyService;
        _referralService = referralService;
    }

    public SteadyGrowth.Web.Models.Entities.User? User { get; set; }
    public IList<SteadyGrowth.Web.Models.Entities.Property> Properties { get; set; } = new List<SteadyGrowth.Web.Models.Entities.Property>();
    public IList<SteadyGrowth.Web.Models.Entities.Referral> Referrals { get; set; } = new List<SteadyGrowth.Web.Models.Entities.Referral>();

    public async Task OnGetAsync(string id)
    {
        User = await _userService.GetUserByIdAsync(id);
        Properties = (await _propertyService.GetUserPropertiesAsync(id)).ToList();
        Referrals = (await _referralService.GetUserReferralsAsync(id)).ToList();
        // TODO: Add audit logging for admin view
    }
}
