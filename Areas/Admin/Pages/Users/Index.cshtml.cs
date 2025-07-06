using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Users;
using SteadyGrowth.Web.Application.Queries.Properties;
using Microsoft.AspNetCore.Mvc;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Users;

/// <summary>
/// User management page model for admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public PaginatedList<UserAdminViewModel>? Users { get; set; }
    public List<AuditLogViewModel> AuditLogs { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = string.Empty;
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10; // Changed to 10 for better pagination display
    public int TotalCount { get; set; }

    public async Task<IActionResult> OnGetAsync(string search = "", string status = "", int pageIndex = 1)
    {
        ViewData["Breadcrumb"] = new List<(string, string)> { ("Admin", "/Admin/Properties/Index"), ("Users", "/Admin/Users/Index") };
        PageIndex = pageIndex;
        SearchTerm = search;
        StatusFilter = status;

        var query = new GetUserListingQuery
        {
            PageIndex = PageIndex,
            PageSize = PageSize,
            SearchTerm = SearchTerm,
            StatusFilter = StatusFilter
        };

        Users = await _mediator.Send(query);
        TotalCount = Users.TotalCount;

        // TODO: Fetch audit logs
        AuditLogs = new List<AuditLogViewModel> {
            new AuditLogViewModel { Timestamp = DateTime.UtcNow, Action = "Viewed user list" }
        };
        return Page();
    }

    // TODO: Implement user activation/deactivation
}
