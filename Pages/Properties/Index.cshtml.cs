using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Pages.Properties;

/// <summary>
/// Property listings page model with search and filtering.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public IndexModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)]
    public PropertyType? PropertyType { get; set; }
    [BindProperty(SupportsGet = true)]
    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }
    [BindProperty(SupportsGet = true)]
    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? Location { get; set; }
    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public IList<Property> Properties { get; set; } = new List<Property>();

    public async Task OnGetAsync()
    {
        await LoadPropertiesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadPropertiesAsync();
        return Page();
    }

    private async Task LoadPropertiesAsync()
    {
        var all = await _propertyService.GetApprovedPropertiesAsync(Page, PageSize);
        var filtered = all.AsQueryable();
        if (!string.IsNullOrWhiteSpace(SearchTerm))
            filtered = filtered.Where(p => p.Title.Contains(SearchTerm) || (p.Location != null && p.Location.Contains(SearchTerm)) || (p.Description != null && p.Description.Contains(SearchTerm)));
        if (PropertyType.HasValue)
            filtered = filtered.Where(p => p.PropertyType == PropertyType);
        if (MinPrice.HasValue)
            filtered = filtered.Where(p => p.Price >= MinPrice);
        if (MaxPrice.HasValue)
            filtered = filtered.Where(p => p.Price <= MaxPrice);
        if (!string.IsNullOrWhiteSpace(Location))
            filtered = filtered.Where(p => p.Location.Contains(Location));
        Properties = filtered.ToList();
    }
}
