using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Queries.Properties
{
    public class GetApprovedPropertyDetailsQuery : IRequest<Property?>
    {
        public int PropertyId { get; set; }

        public class GetApprovedPropertyDetailsQueryHandler : IRequestHandler<GetApprovedPropertyDetailsQuery, Property?>
        {
            private readonly ApplicationDbContext _context;

            public GetApprovedPropertyDetailsQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Property?> Handle(GetApprovedPropertyDetailsQuery request, CancellationToken cancellationToken)
            {
                return await _context.Properties
                    .Include(p => p.PropertyImages)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == request.PropertyId && p.Status == PropertyStatus.Approved, cancellationToken);
            }
        }
    }
}