using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Models.ViewModels;

/// <summary>
/// ViewModel for the admin dashboard.
/// </summary>
public class AdminDashboardViewModel
{
    /// <summary>
    /// Total number of users.
    /// </summary>
    [Display(Name = "Total Users")]
    public int TotalUsers { get; set; }

    /// <summary>
    /// Total number of properties.
    /// </summary>
    [Display(Name = "Total Properties")]
    public int TotalProperties { get; set; }

    /// <summary>
    /// Number of properties pending approval.
    /// </summary>
    [Display(Name = "Pending Properties")]
    public int PendingProperties { get; set; }

    /// <summary>
    /// Number of approved properties.
    /// </summary>
    [Display(Name = "Approved Properties")]
    public int ApprovedProperties { get; set; }

    /// <summary>
    /// Total commissions earned.
    /// </summary>
    [Display(Name = "Total Commissions")]
    public decimal TotalCommissions { get; set; }

    /// <summary>
    /// Monthly statistics (e.g., new users, new properties).
    /// </summary>
    [Display(Name = "Monthly Stats")]
    public Dictionary<string, int> MonthlyStats { get; set; }

    /// <summary>
    /// Recent user activities.
    /// </summary>
    [Display(Name = "Recent Activities")]
    public List<UserActivity> RecentActivities { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminDashboardViewModel"/> class.
    /// </summary>
    public AdminDashboardViewModel()
    {
        MonthlyStats = new Dictionary<string, int>();
        RecentActivities = new List<UserActivity>();
    }
}
