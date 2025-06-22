using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using System.Linq;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Properties;

public class IndexModel : PageModel
{
    private readonly IPropertyService _propertyService;
    public List<PropertyViewModel> Properties { get; set; } = new();

    public IndexModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var all = await _propertyService.GetAllPropertiesAsync();
        Properties = all.Select(p => new PropertyViewModel {
            Id = p.Id,
            Title = p.Title,
            Status = p.Status.ToString(),
            UserEmail = p.User?.Email ?? "",
            CreatedAt = p.CreatedAt
        }).ToList();
        return Page();
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