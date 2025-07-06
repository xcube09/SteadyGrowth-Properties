using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Areas.Admin.Pages.Properties;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Queries.Properties
{
    public class GetAdminPropertyListingQuery : IRequest<PaginatedList<PropertyViewModel>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public PropertyStatus? StatusFilter { get; set; }

        public class GetAdminPropertyListingQueryHandler : IRequestHandler<GetAdminPropertyListingQuery, PaginatedList<PropertyViewModel>>
        {
            private readonly ApplicationDbContext _context;

            public GetAdminPropertyListingQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<PaginatedList<PropertyViewModel>> Handle(GetAdminPropertyListingQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Properties
                    .Include(p => p.User)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(p => p.Title.Contains(request.SearchTerm) ||
                                             p.User.Email.Contains(request.SearchTerm) ||
                                             p.Location.Contains(request.SearchTerm));
                }

                if (request.StatusFilter.HasValue)
                {
                    query = query.Where(p => p.Status == request.StatusFilter.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var properties = await query.Skip((request.PageIndex - 1) * request.PageSize)
                                     .Take(request.PageSize)
                                     .Select(p => new PropertyViewModel
                                     {
                                         Id = p.Id,
                                         Title = p.Title,
                                         Status = p.Status.ToString(),
                                         UserEmail = p.User != null ? p.User.Email : "",
                                         CreatedAt = p.CreatedAt
                                     })
                                     .ToListAsync(cancellationToken);

                return new PaginatedList<PropertyViewModel>(properties, totalCount, request.PageIndex, request.PageSize);
            }
        }
    }
}
