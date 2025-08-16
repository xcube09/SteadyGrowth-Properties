using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Application.Queries.Properties;

public class GetPropertyForEditQuery : IRequest<PropertyEditViewModel?>
{
    public int PropertyId { get; set; }
}

public class GetPropertyForEditQueryHandler : IRequestHandler<GetPropertyForEditQuery, PropertyEditViewModel?>
{
    private readonly ApplicationDbContext _context;

    public GetPropertyForEditQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyEditViewModel?> Handle(GetPropertyForEditQuery request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Include(p => p.PropertyImages.OrderBy(pi => pi.DisplayOrder))
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

        if (property == null)
            return null;

        return new PropertyEditViewModel
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price,
            Location = property.Location,
            PropertyType = property.PropertyType,
            Status = property.Status,
            UserId = property.UserId,
            UserEmail = property.User?.Email ?? string.Empty,
            Features = property.Features,
            IsActive = property.IsActive,
            CreatedAt = property.CreatedAt,
            ApprovedAt = property.ApprovedAt,
            ExistingImages = property.PropertyImages.Select(pi => new PropertyImageEditDto
            {
                Id = pi.Id,
                FileName = pi.FileName,
                Caption = pi.Caption,
                ImageType = pi.ImageType,
                DisplayOrder = pi.DisplayOrder,
                UploadedAt = pi.UploadedAt
            }).ToList()
        };
    }
}

public class PropertyEditViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Location { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
    public PropertyStatus Status { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? Features { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public List<PropertyImageEditDto> ExistingImages { get; set; } = new();
}

public class PropertyImageEditDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? ImageType { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime UploadedAt { get; set; }
}