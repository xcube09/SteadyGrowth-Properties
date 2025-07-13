using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Properties;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using SteadyGrowth.Web.Application.Commands.Properties;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Properties;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public PaginatedList<PropertyViewModel>? Properties { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public PropertyStatus? StatusFilter { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public async Task<IActionResult> OnGetAsync(string search = "", PropertyStatus? status = null, int pageIndex = 1)
    {
        ViewData["Breadcrumb"] = new List<(string, string)> { ("Admin", "/Admin/Properties/Index"), ("Properties", "/Admin/Properties/Index") };

        SearchTerm = search;
        StatusFilter = status;
        PageIndex = pageIndex;

        var query = new GetAdminPropertyListingQuery
        {
            PageIndex = PageIndex,
            PageSize = PageSize,
            SearchTerm = SearchTerm,
            StatusFilter = StatusFilter
        };

        Properties = await _mediator.Send(query);

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var command = new ApprovePropertyCommand { PropertyId = id };
        var result = await _mediator.Send(command);
        if (result)
        {
            TempData["SuccessMessage"] = "Property approved successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to approve property.";
        }
        return RedirectToPage();
    }
}

public class PropertyViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}