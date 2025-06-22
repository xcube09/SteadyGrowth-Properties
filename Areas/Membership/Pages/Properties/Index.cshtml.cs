using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Properties;

/// <summary>
/// User properties page model with filtering and pagination.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public IndexModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public IList<Property> Properties { get; set; } = new List<Property>();
    public PropertyStatus? StatusFilter { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync(PropertyStatus? status, int page = 1)
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId)) return;
        StatusFilter = status;
        Page = page;
        var all = await _propertyService.GetUserPropertiesAsync(userId);
        var filtered = all.AsQueryable();
        if (StatusFilter.HasValue)
            filtered = filtered.Where(p => p.Status == StatusFilter);
        Properties = filtered.Skip((Page - 1) * PageSize).Take(PageSize).ToList();
    }
}
