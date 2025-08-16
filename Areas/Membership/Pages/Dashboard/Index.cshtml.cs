using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Models.Enums;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Data;
using System.Threading.Tasks;
using System.Globalization;

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
    private readonly ICurrentUserService _currentUserService;
    private readonly IWalletService _walletService;
    private readonly IPropertyCommissionService _propertyCommissionService;
    private readonly ApplicationDbContext _context;

	public IndexModel(IUserService userService, IPropertyService propertyService, IReferralService referralService, IRewardService rewardService, ICurrentUserService currentUserService, IWalletService walletService, IPropertyCommissionService propertyCommissionService, ApplicationDbContext context)
	{
		_userService = userService;
		_propertyService = propertyService;
		_referralService = referralService;
		_rewardService = rewardService;
		_currentUserService = currentUserService;
		_walletService = walletService;
		_propertyCommissionService = propertyCommissionService;
		_context = context;
	}

	public UserStats? UserStats { get; set; }
    public ReferralStats? ReferralStats { get; set; }
    public int RewardPoints { get; set; }
    public KYCStatus KYCStatus { get; set; }
    public IList<Models.Entities.Property> RecentProperties { get; set; } = new List<Models.Entities.Property>();
    public Dictionary<int, decimal> PropertyCommissions { get; set; } = new Dictionary<int, decimal>();
    
    // Additional dashboard data
    public User? CurrentUser { get; set; }
    public int DaysSinceJoined { get; set; }
    public decimal WalletBalance { get; set; }
    public int ProfileCompletionPercentage { get; set; }
    public decimal CommissionGrowthPercentage { get; set; }
    public bool HasUpgradeOpportunity { get; set; }
    public UpgradeRequest? PendingUpgradeRequest { get; set; }
    public bool IsPremiumUser { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _currentUserService.GetCurrentUserAsync();
        if (user is null) return;
        
        CurrentUser = user;
        UserStats = await _userService.GetUserStatsAsync(user.Id);
        ReferralStats = await _referralService.GetReferralStatsAsync(user.Id);
        RewardPoints = await _rewardService.GetUserTotalPointsAsync(user.Id);
        RecentProperties = (await _propertyCommissionService.GetUserCommissionPropertiesAsync(user.Id)).Take(5).ToList();
        KYCStatus = user.KYCStatus;
        
        // Get commission amounts for each property
        foreach (var property in RecentProperties)
        {
            PropertyCommissions[property.Id] = await _propertyCommissionService.GetUserPropertyCommissionAsync(user.Id, property.Id);
        }
        
        // Calculate additional dashboard metrics  
        DaysSinceJoined = Math.Max(0, (DateTime.UtcNow.Date - user.CreatedAt.Date).Days);
        
        // Get wallet balance
        var wallet = await _walletService.GetWalletByUserIdAsync(user.Id);
        WalletBalance = wallet?.Balance ?? 0;
        
        // Calculate profile completion percentage
        ProfileCompletionPercentage = CalculateProfileCompletion(user);
        
        // Calculate commission growth (simplified example)
        CommissionGrowthPercentage = ReferralStats?.TotalCommissionEarned > 0 ? 15.5m : 0;
        
        // Check if user is premium
        IsPremiumUser = user.AcademyPackage?.Name == "Premium Package";
        
        // Check if user can upgrade academy package (not premium and no pending request)
        HasUpgradeOpportunity = !IsPremiumUser;
        
        // Check for pending upgrade request
        PendingUpgradeRequest = await _context.UpgradeRequests
            .Include(ur => ur.RequestedPackage)
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.Status == UpgradeRequestStatus.Pending);
            
        // If there's a pending request, disable upgrade opportunity
        if (PendingUpgradeRequest != null)
        {
            HasUpgradeOpportunity = false;
        }
    }
    
    private int CalculateProfileCompletion(User user)
    {
        var completionScore = 0;
        var totalFields = 8;
        
        if (!string.IsNullOrEmpty(user.FirstName)) completionScore++;
        if (!string.IsNullOrEmpty(user.LastName)) completionScore++;
        if (!string.IsNullOrEmpty(user.Email)) completionScore++;
        if (!string.IsNullOrEmpty(user.PhoneNumber)) completionScore++;
        if (!string.IsNullOrEmpty(user.ProfilePictureUrl)) completionScore++;
        if (!string.IsNullOrEmpty(user.ReferralCode)) completionScore++;
        if (user.KYCStatus == KYCStatus.Approved) completionScore++;
        if (user.AcademyPackageId.HasValue) completionScore++;
        
        return (completionScore * 100) / totalFields;
    }

    
}
