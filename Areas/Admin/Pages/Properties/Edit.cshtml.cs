using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Properties;
using SteadyGrowth.Web.Application.Commands.Properties;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Properties;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly IMediator _mediator;

    public EditModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public PropertyEditCommand Command { get; set; } = new();

    public PropertyEditViewModel? Property { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Breadcrumb"] = new List<(string, string)> 
        { 
            ("Admin", "/Admin/Properties/Index"), 
            ("Properties", "/Admin/Properties/Index"),
            ("Edit Property", $"/Admin/Properties/Edit/{id}")
        };

        var query = new GetPropertyForEditQuery { PropertyId = id };
        Property = await _mediator.Send(query);

        if (Property == null)
        {
            TempData["ErrorMessage"] = "Property not found.";
            return RedirectToPage("./Index");
        }

        // Populate command with existing property data
        Command = new PropertyEditCommand
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

        var adminCommand = new AdminUpdatePropertyCommand
        {
            Id = Command.Id,
            Title = Command.Title,
            Description = Command.Description,
            Price = Command.Price,
            Location = Command.Location,
            PropertyType = Command.PropertyType,
            Status = Command.Status,
            Features = Command.Features,
            IsActive = Command.IsActive,
            ExistingImages = Command.ExistingImages,
            NewImages = Command.NewImages,
            ImagesToDelete = Command.ImagesToDelete
        };

        var result = await _mediator.Send(adminCommand);

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

    public async Task<IActionResult> OnPostUploadImageAsync()
    {
        var file = Request.Form.Files.FirstOrDefault();
        if (file == null || file.Length == 0)
        {
            return new JsonResult(new { success = false, message = "No file uploaded" });
        }

        // Validate file type and size
        var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedTypes.Contains(fileExtension))
        {
            return new JsonResult(new { success = false, message = "Invalid file type. Only JPG, PNG, and GIF files are allowed." });
        }

        if (file.Length > 50 * 1024 * 1024) // 50MB limit
        {
            return new JsonResult(new { success = false, message = "File size exceeds 50MB limit." });
        }

        try
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "properties");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new JsonResult(new 
            { 
                success = true, 
                fileName = uniqueFileName,
                originalName = file.FileName,
                fileSize = file.Length
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error uploading file: " + ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteImageAsync(int imageId)
    {
        try
        {
            // Add logic to delete physical file if needed
            // For now, we'll just return success - the actual deletion happens during property update
            
            return new JsonResult(new { success = true, message = "Image marked for deletion" });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error deleting image: " + ex.Message });
        }
    }
}

public class PropertyEditCommand
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters.")]
    public string Location { get; set; } = string.Empty;

    public PropertyType PropertyType { get; set; }
    public PropertyStatus Status { get; set; }
    public string Features { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Image management for form binding
    public List<PropertyImageCommandDto> NewImages { get; set; } = new();
    public List<PropertyImageUpdateDto> ExistingImages { get; set; } = new();
    public List<int> ImagesToDelete { get; set; } = new();
}