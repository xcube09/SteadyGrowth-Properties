using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Properties;
using Microsoft.AspNetCore.Identity;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Properties;

/// <summary>
/// User properties page model with filtering and pagination.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<User> _userManager;

    public IndexModel(IMediator mediator, UserManager<User> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    public PaginatedList<Property>? Properties { get; set; }
    public PropertyStatus? StatusFilter { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync(PropertyStatus? status, int pageIndex = 1)
    {
        ViewData["Breadcrumb"] = new List<(string, string)> { ("My Properties", "/Membership/Properties/Index") };
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return;

        StatusFilter = status;
        PageIndex = pageIndex;

        var query = new GetMemberPropertyListingQuery
        {
            UserId = userId,
            PageIndex = PageIndex,
            PageSize = PageSize,
            StatusFilter = StatusFilter
        };

        Properties = await _mediator.Send(query);
    }
}
