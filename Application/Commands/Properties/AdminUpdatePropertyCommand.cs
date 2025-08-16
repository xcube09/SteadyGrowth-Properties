using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Application.Commands.Properties;

public class AdminUpdatePropertyCommand : IRequest<bool>
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
    public string? Features { get; set; }
    public bool IsActive { get; set; } = true;

    // Image management
    public List<PropertyImageCommandDto> NewImages { get; set; } = new();
    public List<PropertyImageUpdateDto> ExistingImages { get; set; } = new();
    public List<int> ImagesToDelete { get; set; } = new();
}

public class PropertyImageUpdateDto
{
    public int Id { get; set; }
    public string? Caption { get; set; }
    public string? ImageType { get; set; }
    public int DisplayOrder { get; set; }
}

public class AdminUpdatePropertyCommandHandler : IRequestHandler<AdminUpdatePropertyCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public AdminUpdatePropertyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AdminUpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        try
        {
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

            // Set approval date if status changed to approved
            if (request.Status == PropertyStatus.Approved && property.ApprovedAt == null)
            {
                property.ApprovedAt = DateTime.UtcNow;
            }

            // Handle image deletions first
            if (request.ImagesToDelete.Any())
            {
                var imagesToRemove = property.PropertyImages
                    .Where(pi => request.ImagesToDelete.Contains(pi.Id))
                    .ToList();

                foreach (var imageToRemove in imagesToRemove)
                {
                    _context.PropertyImages.Remove(imageToRemove);
                }
            }

            // Update existing images
            foreach (var existingImageDto in request.ExistingImages)
            {
                var image = property.PropertyImages.FirstOrDefault(pi => pi.Id == existingImageDto.Id);
                if (image != null)
                {
                    image.Caption = existingImageDto.Caption;
                    image.ImageType = existingImageDto.ImageType;
                    image.DisplayOrder = existingImageDto.DisplayOrder;
                }
            }

            // Add new images
            foreach (var newImageDto in request.NewImages)
            {
                var propertyImage = new PropertyImage
                {
                    PropertyId = property.Id,
                    FileName = newImageDto.FileName,
                    Caption = newImageDto.Caption,
                    ImageType = newImageDto.ImageType,
                    DisplayOrder = newImageDto.DisplayOrder,
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