using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Users;

/// <summary>
/// User management page model for admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;

    public IndexModel(IUserService userService)
    {
        _userService = userService;
    }

    public List<UserAdminViewModel> Users { get; set; } = new();
    public List<AuditLogViewModel> AuditLogs { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }

    public async Task OnGetAsync(string search = "", string status = "", int page = 1)
    {
        Page = page;
        SearchTerm = search;
        StatusFilter = status;
        // Fetch users and map to view model
        var users = await _userService.GetAllUsersAsync(page, PageSize);
        Users = users.Select(u => new UserAdminViewModel {
            Id = u.Id,
            FullName = u.FirstName + " " + u.LastName,
            Email = u.Email,
            Status = u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow ? "Suspended" : "Active",
            Role = "User", // TODO: fetch actual role
            RegisteredAt = u.CreatedAt
        }).ToList();
        TotalCount = await _userService.GetTotalUsersCountAsync();
        // TODO: Fetch audit logs
        AuditLogs = new List<AuditLogViewModel> {
            new AuditLogViewModel { Timestamp = DateTime.UtcNow, Action = "Viewed user list" }
        };
    }

    // TODO: Implement user activation/deactivation
}
