using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Application.Queries.UpgradeRequests
{
    public class GetUpgradeRequestsQuery : IRequest<IList<UpgradeRequest>>
    {
        public UpgradeRequestStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetUpgradeRequestsQueryHandler : IRequestHandler<GetUpgradeRequestsQuery, IList<UpgradeRequest>>
    {
        private readonly ApplicationDbContext _context;

        public GetUpgradeRequestsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<UpgradeRequest>> Handle(GetUpgradeRequestsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.UpgradeRequests
                .Include(ur => ur.User)
                .Include(ur => ur.RequestedPackage)
                .AsQueryable();

            // Apply filters
            if (request.Status.HasValue)
            {
                query = query.Where(ur => ur.Status == request.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(ur => 
                    ur.User.FirstName.Contains(request.SearchTerm) ||
                    ur.User.LastName.Contains(request.SearchTerm) ||
                    ur.User.Email.Contains(request.SearchTerm) ||
                    ur.PaymentDetails.Contains(request.SearchTerm));
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(ur => ur.RequestedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(ur => ur.RequestedAt <= request.ToDate.Value.AddDays(1));
            }

            return await query
                .OrderByDescending(ur => ur.RequestedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
        }
    }
}