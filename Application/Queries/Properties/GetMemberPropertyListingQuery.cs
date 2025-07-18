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
    public class GetMemberPropertyListingQuery : IRequest<PaginatedList<Property>>
    {
        public string? UserId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public PropertyStatus? StatusFilter { get; set; }

        public class GetMemberPropertyListingQueryHandler : IRequestHandler<GetMemberPropertyListingQuery, PaginatedList<Property>>
        {
            private readonly ApplicationDbContext _context;

            public GetMemberPropertyListingQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<PaginatedList<Property>> Handle(GetMemberPropertyListingQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Properties
                    .Where(p => p.UserId == request.UserId)
                    .Include(p => p.PropertyImages)
                    .AsQueryable();

                if (request.StatusFilter.HasValue)
                {
                    query = query.Where(p => p.Status == request.StatusFilter.Value);
                }

                var count = await query.CountAsync(cancellationToken);
                var items = await query.Skip((request.PageIndex - 1) * request.PageSize)
                                     .Take(request.PageSize)
                                     .ToListAsync(cancellationToken);

                return new PaginatedList<Property>(items, count, request.PageIndex, request.PageSize);
            }
        }
    }
}
