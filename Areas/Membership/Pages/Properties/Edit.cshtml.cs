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
/// Edit property page model for members.
/// </summary>
[Authorize]
public class EditModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public EditModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [BindProperty]
    public int Id { get; set; }
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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var property = await _propertyService.GetPropertyByIdAsync(id);
        if (property == null || property.UserId != userId)
            return NotFound();
        Id = property.Id;
        Title = property.Title;
        Description = property.Description;
        Location = property.Location;
        Price = property.Price;
        PropertyType = property.PropertyType;
        // TODO: Load images
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var userId = User.Identity?.Name;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var property = await _propertyService.GetPropertyByIdAsync(Id);
        if (property == null || property.UserId != userId)
            return NotFound();
        property.Title = Title;
        property.Description = Description;
        property.Location = Location;
        property.Price = Price;
        property.PropertyType = PropertyType;
        // TODO: Handle image update
        await _propertyService.UpdatePropertyAsync(property);
        ResultMessage = "Property updated successfully.";
        return RedirectToPage("Index");
    }
}
