using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Models.ViewModels;

/// <summary>
/// ViewModel for user management in the admin area.
/// </summary>
public class UserManagementViewModel
{
    /// <summary>
    /// List of users.
    /// </summary>
    [Display(Name = "Users")]
    public IEnumerable<User> Users { get; set; }

    /// <summary>
    /// Search term for filtering users.
    /// </summary>
    [Display(Name = "Search Term")]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by active status.
    /// </summary>
    [Display(Name = "Is Active")]
    public bool? IsActive { get; set; }

    /// <summary>
    /// Total number of users.
    /// </summary>
    [Display(Name = "Total Count")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number.
    /// </summary>
    [Display(Name = "Current Page")]
    public int CurrentPage { get; set; }

    /// <summary>
    /// Number of users per page.
    /// </summary>
    [Display(Name = "Page Size")]
    public int PageSize { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserManagementViewModel"/> class.
    /// </summary>
    public UserManagementViewModel()
    {
        Users = Enumerable.Empty<User>();
        CurrentPage = 1;
        PageSize = 50;
    }
}
