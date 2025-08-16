using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Application.Queries.UpgradeRequests
{
    public class GetPendingUpgradeRequestsQuery : IRequest<IList<UpgradeRequest>>
    {
    }

    public class GetPendingUpgradeRequestsQueryHandler : IRequestHandler<GetPendingUpgradeRequestsQuery, IList<UpgradeRequest>>
    {
        private readonly ApplicationDbContext _context;

        public GetPendingUpgradeRequestsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<UpgradeRequest>> Handle(GetPendingUpgradeRequestsQuery request, CancellationToken cancellationToken)
        {
            return await _context.UpgradeRequests
                .Include(ur => ur.User)
                .Include(ur => ur.RequestedPackage)
                .Where(ur => ur.Status == UpgradeRequestStatus.Pending)
                .OrderByDescending(ur => ur.RequestedAt)
                .ToListAsync(cancellationToken);
        }
    }
}