using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Pages.Properties;

/// <summary>
/// Property details page model.
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public DetailsModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public Property? Property { get; set; }
    public IList<Property> RelatedProperties { get; set; } = new List<Property>();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Property = await _propertyService.GetPropertyByIdAsync(id);
        if (Property == null || Property.Status != PropertyStatus.Approved)
            return NotFound();
        // Related properties: same location and type, exclude current
        var all = await _propertyService.GetApprovedPropertiesAsync(1, 12);
        RelatedProperties = all.Where(p => p.Id != id && p.Location == Property.Location && p.PropertyType == Property.PropertyType).Take(4).ToList();
        return Page();
    }
}
