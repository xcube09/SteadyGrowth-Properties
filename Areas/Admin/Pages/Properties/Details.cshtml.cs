using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Properties;

/// <summary>
/// Property vetting details and history for admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class DetailsModel : PageModel
{
    private readonly IPropertyService _propertyService;
    private readonly IVettingService _vettingService;

    public DetailsModel(IPropertyService propertyService, IVettingService vettingService)
    {
        _propertyService = propertyService;
        _vettingService = vettingService;
    }

    public Property? Property { get; set; }
    public IList<VettingLog> VettingHistory { get; set; } = new List<VettingLog>();

    public async Task OnGetAsync(int id)
    {
        Property = await _propertyService.GetPropertyByIdAsync(id);
        VettingHistory = (await _vettingService.GetVettingHistoryAsync(id)).ToList();
        // TODO: Add audit logging for admin view
    }
}
