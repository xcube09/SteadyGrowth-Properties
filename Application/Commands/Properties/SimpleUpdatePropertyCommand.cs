using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Application.Commands.Properties;

public class SimpleUpdatePropertyCommand : IRequest<bool>
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

    // Image management
    public List<PropertyImageCommandDto> NewImages { get; set; } = new();
    public List<int> ImagesToDelete { get; set; } = new();
    public List<PropertyImageUpdateDto> ExistingImages { get; set; } = new();
}

public class SimpleUpdatePropertyCommandHandler : IRequestHandler<SimpleUpdatePropertyCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public SimpleUpdatePropertyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SimpleUpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the property with its images
            var property = await _context.Properties
                .Include(p => p.PropertyImages)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (property == null)
                return false;

            // Update basic property information
            property.Title = request.Title;
            property.Description = request.Description;
            property.Price = request.Price;
            property.Location = request.Location;
            property.PropertyType = request.PropertyType;
            property.Status = request.Status;
            property.Features = request.Features;
            property.IsActive = request.IsActive;

            // Handle image deletions
            if (request.ImagesToDelete.Any())
            {
                var imagesToRemove = property.PropertyImages
                    .Where(img => request.ImagesToDelete.Contains(img.Id))
                    .ToList();

                foreach (var image in imagesToRemove)
                {
                    // Delete physical file
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "properties", image.FileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                _context.PropertyImages.RemoveRange(imagesToRemove);
            }

            // Update existing image properties (captions, display order, etc.)
            foreach (var existingImageUpdate in request.ExistingImages)
            {
                var existingImage = property.PropertyImages.FirstOrDefault(img => img.Id == existingImageUpdate.Id);
                if (existingImage != null)
                {
                    existingImage.Caption = existingImageUpdate.Caption ?? string.Empty;
                    existingImage.ImageType = existingImageUpdate.ImageType ?? string.Empty;
                    existingImage.DisplayOrder = existingImageUpdate.DisplayOrder;
                }
            }

            // Add new images
            foreach (var newImage in request.NewImages)
            {
                var propertyImage = new PropertyImage
                {
                    PropertyId = property.Id,
                    FileName = newImage.FileName,
                    Caption = newImage.Caption ?? string.Empty,
                    ImageType = newImage.ImageType ?? string.Empty,
                    DisplayOrder = newImage.DisplayOrder,
                    UploadedAt = DateTime.UtcNow
                };

                property.PropertyImages.Add(propertyImage);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}