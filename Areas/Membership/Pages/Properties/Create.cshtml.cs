using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Properties;

/// <summary>
/// Add property page model for members.
/// </summary>
[Authorize]
public class CreateModel : PageModel
{
    private readonly IPropertyService _propertyService;
    private readonly IVettingService _vettingService;

    public CreateModel(IPropertyService propertyService, IVettingService vettingService)
    {
        _propertyService = propertyService;
        _vettingService = vettingService;
    }

    [BindProperty]
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;
    [BindProperty]
    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    [BindProperty]
    [Required, StringLength(500)]
    public string Location { get; set; } = string.Empty;
    [BindProperty]
    [Required]
    public decimal Price { get; set; }
    [BindProperty]
    [Required]
    public PropertyType PropertyType { get; set; }
    [BindProperty]
    public List<IFormFile> Images { get; set; } = new();

    [TempData]
    public string? ResultMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
        {
            ResultMessage = "User not authenticated.";
            return Page();
        }
        var property = new Property
        {
            Title = Title,
            Description = Description,
            Location = Location,
            Price = Price,
            PropertyType = PropertyType,
            UserId = userId
        };
        // TODO: Handle image upload and set property.Images as JSON array
        await _propertyService.CreatePropertyAsync(property, userId);
        await _vettingService.SubmitForVettingAsync(property.Id);
        ResultMessage = "Property created and submitted for vetting.";
        return RedirectToPage("Index");
    }
}
