using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;

namespace SteadyGrowth.Web.Application.Commands.Properties;

public class DeletePropertyCommand : IRequest<bool>
{
    public int PropertyId { get; set; }
}

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public DeletePropertyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

            if (property == null)
            {
                return false;
            }

            // Delete related images first
            var images = await _context.PropertyImages
                .Where(pi => pi.PropertyId == request.PropertyId)
                .ToListAsync(cancellationToken);

            if (images.Any())
            {
                _context.PropertyImages.RemoveRange(images);
            }

            // Delete the property
            _context.Properties.Remove(property);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}