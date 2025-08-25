using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Models.ViewModels;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Users;
using SteadyGrowth.Web.Application.Queries.Properties;
using SteadyGrowth.Web.Application.Queries.Academy;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using System.Text.Json;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Users;

/// <summary>
/// User management page model for admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IWalletService _walletService;

    public IndexModel(IMediator mediator, IWalletService walletService)
    {
        _mediator = mediator;
        _walletService = walletService;
    }

    public PaginatedList<UserAdminViewModel>? Users { get; set; }
    public List<AuditLogViewModel> AuditLogs { get; set; } = new();
    public IList<AcademyPackage> AvailablePackages { get; set; } = new List<AcademyPackage>();
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = string.Empty;
    public int? PackageFilter { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10; // Changed to 10 for better pagination display
    public int TotalCount { get; set; }

    public async Task<IActionResult> OnGetAsync(string search = "", string status = "", int? packageFilter = null, int pageIndex = 1)
    {
        ViewData["Breadcrumb"] = new List<(string, string)> { ("Admin", "/Admin/Properties/Index"), ("Users", "/Admin/Users/Index") };
        PageIndex = pageIndex;
        SearchTerm = search;
        StatusFilter = status;
        PackageFilter = packageFilter;

        // Load available packages for filter dropdown
        AvailablePackages = await _mediator.Send(new GetAvailableAcademyPackagesQuery());

        var query = new GetUserListingQuery
        {
            PageIndex = PageIndex,
            PageSize = PageSize,
            SearchTerm = SearchTerm,
            StatusFilter = StatusFilter,
            PackageFilter = PackageFilter
        };

        Users = await _mediator.Send(query);
        TotalCount = Users?.TotalCount ?? 0;

        // TODO: Fetch audit logs
        AuditLogs = new List<AuditLogViewModel> {
            new AuditLogViewModel { Timestamp = DateTime.UtcNow, Action = "Viewed user list" }
        };
        return Page();
    }

    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> OnPostWalletActionAsync()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var data = System.Text.Json.JsonDocument.Parse(body).RootElement;
            var userId = data.GetProperty("userId").GetString();
            var type = data.GetProperty("type").GetString();
            var amount = data.GetProperty("amount").GetDecimal();
            var description = data.GetProperty("description").GetString();
            if (string.IsNullOrWhiteSpace(userId) || amount <= 0 || string.IsNullOrWhiteSpace(type))
                return new JsonResult(new { success = false, message = "Invalid input." });
            if (type == "credit")
            {
                await _walletService.CreditWalletAsync(userId, amount, description, User.Identity?.Name);
            }
            else if (type == "debit")
            {
                await _walletService.DebitWalletAsync(userId, amount, description);
            }
            else
            {
                return new JsonResult(new { success = false, message = "Invalid type." });
            }
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> OnPostDeleteAsync()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var data = JsonDocument.Parse(body).RootElement;
            var userId = data.GetProperty("Id").GetString();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return new JsonResult(new { success = false, message = "Invalid user ID." });
            }

            var result = await _mediator.Send(new SteadyGrowth.Web.Application.Commands.Users.DeleteUserCommand { UserId = userId });

            if (result)
            {
                return new JsonResult(new { success = true, message = "User deleted successfully." });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Failed to delete user." });
            }
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error deleting user: " + ex.Message });
        }
    }

    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> OnPostDeleteSelectedAsync()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var userIds = JsonSerializer.Deserialize<List<string>>(body);

            if (userIds == null || !userIds.Any())
            {
                return new JsonResult(new { success = false, message = "No users selected for deletion." });
            }

            var result = await _mediator.Send(new SteadyGrowth.Web.Application.Commands.Users.DeleteMultipleUsersCommand { UserIds = userIds });

            if (result)
            {
                return new JsonResult(new { success = true, message = "Selected users deleted successfully." });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Failed to delete selected users." });
            }
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error deleting users: " + ex.Message });
        }
    }

    // TODO: Implement user activation/deactivation
}
