using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Application.Commands.Properties
{
    public class AddAdminPropertyCommand : IRequest<Property>
    {
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
        public List<PropertyImageCommandDto> Images { get; set; } = new List<PropertyImageCommandDto>();

        public class AddAdminPropertyCommandHandler : IRequestHandler<AddAdminPropertyCommand, Property>
        {
            private readonly ApplicationDbContext _context;

            public AddAdminPropertyCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Property> Handle(AddAdminPropertyCommand request, CancellationToken cancellationToken)
            {
                var property = new Property
                {
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    Location = request.Location,
                    UserId = request.UserId,
                    Status = PropertyStatus.Approved, // Admin properties are approved by default
                    ApprovedAt = DateTime.UtcNow // Set approval time immediately
                };

                _context.Properties.Add(property);
                await _context.SaveChangesAsync(cancellationToken);

                foreach (var imageDto in request.Images)
                {
                    var propertyImage = new PropertyImage
                    {
                        PropertyId = property.Id,
                        FileName = imageDto.FileName!,
                        Caption = imageDto.Caption,
                        ImageType = imageDto.ImageType,
                        DisplayOrder = imageDto.DisplayOrder
                    };
                    _context.PropertyImages.Add(propertyImage);
                }
                await _context.SaveChangesAsync(cancellationToken);

                return property;
            }
        }
    }
} 