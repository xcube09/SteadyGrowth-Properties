using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Properties;
using SteadyGrowth.Web.Application.Commands.Properties;
using SteadyGrowth.Web.Application.ViewModels;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Properties;

[Authorize(Roles = "Admin")]
public class EditSimpleModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EditSimpleModel(IMediator mediator, IWebHostEnvironment webHostEnvironment)
    {
        _mediator = mediator;
        _webHostEnvironment = webHostEnvironment;
    }

    [BindProperty]
    public SimpleUpdatePropertyCommand Command { get; set; } = new();

    [BindProperty]
    public List<PropertyImageUploadModel> NewImages { get; set; } = new();

    public PropertyEditViewModel? Property { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Breadcrumb"] = new List<(string, string)> 
        { 
            ("Admin", "/Admin/Properties/Index"), 
            ("Properties", "/Admin/Properties/Index"),
            ("Edit Property", $"/Admin/Properties/EditSimple/{id}")
        };

        var query = new GetPropertyForEditQuery { PropertyId = id };
        Property = await _mediator.Send(query);

        if (Property == null)
        {
            TempData["ErrorMessage"] = "Property not found.";
            return RedirectToPage("./Index");
        }

        // Populate command with existing property data
        Command = new SimpleUpdatePropertyCommand
        {
            Id = Property.Id,
            Title = Property.Title,
            Description = Property.Description,
            Price = Property.Price,
            Location = Property.Location,
            PropertyType = Property.PropertyType,
            Status = Property.Status,
            Features = Property.Features ?? string.Empty,
            IsActive = Property.IsActive,
            ExistingImages = Property.ExistingImages.Select(img => new PropertyImageUpdateDto
            {
                Id = img.Id,
                Caption = img.Caption,
                ImageType = img.ImageType,
                DisplayOrder = img.DisplayOrder
            }).ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Reload property data for display
            var query = new GetPropertyForEditQuery { PropertyId = Command.Id };
            Property = await _mediator.Send(query);
            return Page();
        }

        // Process new image uploads
        foreach (var image in NewImages)
        {
            if (image.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.ImageFile.CopyToAsync(fileStream);
                }

                Command.NewImages.Add(new PropertyImageCommandDto
                {
                    FileName = uniqueFileName,
                    Caption = image.Caption ?? string.Empty,
                    ImageType = image.ImageType ?? string.Empty,
                    DisplayOrder = image.DisplayOrder
                });
            }
        }

        var result = await _mediator.Send(Command);

        if (result)
        {
            TempData["PropertySuccessMessage"] = "Property updated successfully.";
            return RedirectToPage("./Index");
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update property. Please try again.";
            
            // Reload property data for display
            var query = new GetPropertyForEditQuery { PropertyId = Command.Id };
            Property = await _mediator.Send(query);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteImageAsync(int imageId)
    {
        try
        {
            // Just return success - actual deletion happens during property update
            return new JsonResult(new { success = true, message = "Image will be deleted on save" });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error preparing image deletion: " + ex.Message });
        }
    }
}