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
        public string? TitleFilter { get; set; }
        public string? LocationFilter { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

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

                // Apply title filter
                if (!string.IsNullOrEmpty(request.TitleFilter))
                {
                    query = query.Where(p => p.Title.Contains(request.TitleFilter));
                }

                // Apply location filter
                if (!string.IsNullOrEmpty(request.LocationFilter))
                {
                    query = query.Where(p => p.Location.Contains(request.LocationFilter));
                }

                // Apply price range filters
                if (request.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= request.MinPrice.Value);
                }

                if (request.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= request.MaxPrice.Value);
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
