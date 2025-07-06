using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Queries.Properties
{
    public class GetPropertyDetailsQuery : IRequest<Property>
    {
        public int PropertyId { get; set; }
        public string? UserId { get; set; }

        public class GetPropertyDetailsQueryHandler : IRequestHandler<GetPropertyDetailsQuery, Property>
        {
            private readonly ApplicationDbContext _context;

            public GetPropertyDetailsQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Property> Handle(GetPropertyDetailsQuery request, CancellationToken cancellationToken)
            {
                return await _context.Properties
                    .Include(p => p.PropertyImages)
                    .FirstOrDefaultAsync(p => p.Id == request.PropertyId && p.UserId == request.UserId, cancellationToken);
            }
        }
    }
}
