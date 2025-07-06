using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace SteadyGrowth.Web.Application.Commands.Properties
{
    public class ApprovePropertyCommand : IRequest<bool>
    {
        public int PropertyId { get; set; }

        public class ApprovePropertyCommandHandler : IRequestHandler<ApprovePropertyCommand, bool>
        {
            private readonly ApplicationDbContext _context;

            public ApprovePropertyCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<bool> Handle(ApprovePropertyCommand request, CancellationToken cancellationToken)
            {
                var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

                if (property == null)
                {
                    return false;
                }

                property.Status = PropertyStatus.Approved;
                property.ApprovedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }
}
