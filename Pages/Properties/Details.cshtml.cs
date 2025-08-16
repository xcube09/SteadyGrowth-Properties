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
    private readonly ICurrencyService _currencyService;

    public DetailsModel(IPropertyService propertyService, ICurrencyService currencyService)
    {
        _propertyService = propertyService;
        _currencyService = currencyService;
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

    public async Task<string> GetFormattedPriceWithUSDAsync(Property property)
    {
        var currencyCode = property.CurrencyCode ?? "USD";
        var currencySymbol = await _currencyService.GetCurrencySymbolAsync(currencyCode);
        var originalPrice = $"{currencySymbol}{property.Price:N2}";

        if (currencyCode == "USD")
        {
            return originalPrice;
        }

        try
        {
            var usdPrice = await _currencyService.ConvertAmountAsync(property.Price, currencyCode, "USD");
            return $"{originalPrice} (${usdPrice:N2} USD)";
        }
        catch
        {
            return originalPrice;
        }
    }
}
