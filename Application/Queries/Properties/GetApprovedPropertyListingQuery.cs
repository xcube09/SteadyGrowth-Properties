using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Queries.Properties
{
    public class GetApprovedPropertyListingQuery : IRequest<PaginatedList<Property>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public PropertyStatus? StatusFilter { get; set; }

        public class GetApprovedPropertyListingQueryHandler : IRequestHandler<GetApprovedPropertyListingQuery, PaginatedList<Property>>
        {
            private readonly ApplicationDbContext _context;

            public GetApprovedPropertyListingQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<PaginatedList<Property>> Handle(GetApprovedPropertyListingQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Properties
                    .Where(p => p.Status == PropertyStatus.Approved)
                    .AsQueryable();

                if (request.StatusFilter.HasValue)
                {
                    query = query.Where(p => p.Status == request.StatusFilter.Value);
                }

                query = query.Include(p => p.PropertyImages);

                var count = await query.CountAsync(cancellationToken);
                var items = await query.Skip((request.PageIndex - 1) * request.PageSize)
                                     .Take(request.PageSize)
                                     .ToListAsync(cancellationToken);

                return new PaginatedList<Property>(items, count, request.PageIndex, request.PageSize);
            }
        }
    }
}
