using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Models.ViewModels;

/// <summary>
/// ViewModel for the membership dashboard page.
/// </summary>
public class DashboardViewModel
{
    /// <summary>
    /// User statistics.
    /// </summary>
    [Display(Name = "User Statistics")]
    public UserStats UserStats { get; set; }

    /// <summary>
    /// Recent properties listed by the user.
    /// </summary>
    [Display(Name = "Recent Properties")]
    public List<Property> RecentProperties { get; set; }

    /// <summary>
    /// Referral statistics.
    /// </summary>
    [Display(Name = "Referral Statistics")]
    public ReferralStats ReferralStats { get; set; }

    /// <summary>
    /// Total reward points earned by the user.
    /// </summary>
    [Display(Name = "Total Reward Points")]
    public int TotalRewardPoints { get; set; }

    /// <summary>
    /// Recent user activities.
    /// </summary>
    [Display(Name = "Recent Activities")]
    public List<UserActivity> RecentActivities { get; set; }

    /// <summary>
    /// User's referral link.
    /// </summary>
    [Display(Name = "Referral Link")]
    public string ReferralLink { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    public DashboardViewModel()
    {
        UserStats = new UserStats();
        RecentProperties = new List<Property>();
        ReferralStats = new ReferralStats();
        RecentActivities = new List<UserActivity>();
        ReferralLink = string.Empty;
    }
}
