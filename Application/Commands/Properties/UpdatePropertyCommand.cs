using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Commands.Properties
{
    public class UpdatePropertyCommand : IRequest<Property>
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters.")]
        public required string Location { get; set; }

        public string? UserId { get; set; }

        public List<PropertyImageCommandDto> NewImages { get; set; } = new List<PropertyImageCommandDto>();
        public List<PropertyImageCommandDto> ExistingImages { get; set; } = new List<PropertyImageCommandDto>();
        public List<int> ImagesToDelete { get; set; } = new List<int>();

        public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, Property>
        {
            private readonly ApplicationDbContext _context;

            public UpdatePropertyCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Property> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
            {
                var property = await _context.Properties
                    .Include(p => p.PropertyImages)
                    .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == request.UserId, cancellationToken);

                if (property == null)
                {
                    return null; // Or throw an exception
                }

                property.Title = request.Title;
                property.Description = request.Description;
                property.Price = request.Price;
                property.Location = request.Location;

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
                    property.PropertyImages.Add(new PropertyImage
                    {
                        FileName = newImageDto.FileName,
                        Caption = newImageDto.Caption,
                        ImageType = newImageDto.ImageType,
                        DisplayOrder = newImageDto.DisplayOrder,
                        UploadedAt = DateTime.UtcNow
                    });
                }

                // Remove images marked for deletion
                foreach (var imageIdToDelete in request.ImagesToDelete)
                {
                    var imageToRemove = property.PropertyImages.FirstOrDefault(pi => pi.Id == imageIdToDelete);
                    if (imageToRemove != null)
                    {
                        _context.PropertyImages.Remove(imageToRemove);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                return property;
            }
        }
    }
}
