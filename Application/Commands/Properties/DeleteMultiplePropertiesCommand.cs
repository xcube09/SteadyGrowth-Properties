using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using System.Collections.Generic;
using System.Linq;

namespace SteadyGrowth.Web.Application.Commands.Properties;

public class DeleteMultiplePropertiesCommand : IRequest<bool>
{
    public List<int> PropertyIds { get; set; } = new();
}

public class DeleteMultiplePropertiesCommandHandler : IRequestHandler<DeleteMultiplePropertiesCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public DeleteMultiplePropertiesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteMultiplePropertiesCommand request, CancellationToken cancellationToken)
    {
        if (request.PropertyIds == null || !request.PropertyIds.Any())
        {
            return false;
        }

        try
        {
            // Get all properties to delete
            var properties = await _context.Properties
                .Where(p => request.PropertyIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            if (!properties.Any())
            {
                return false;
            }

            // Delete related images
            var propertyIds = properties.Select(p => p.Id).ToList();
            var images = await _context.PropertyImages
                .Where(pi => propertyIds.Contains(pi.PropertyId))
                .ToListAsync(cancellationToken);

            if (images.Any())
            {
                _context.PropertyImages.RemoveRange(images);
            }

            // Delete the properties
            _context.Properties.RemoveRange(properties);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}