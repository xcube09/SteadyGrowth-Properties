using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Application.Queries.UpgradeRequests
{
    public class GetUpgradeRequestByIdQuery : IRequest<UpgradeRequest?>
    {
        public int Id { get; set; }
    }

    public class GetUpgradeRequestByIdQueryHandler : IRequestHandler<GetUpgradeRequestByIdQuery, UpgradeRequest?>
    {
        private readonly ApplicationDbContext _context;

        public GetUpgradeRequestByIdQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UpgradeRequest?> Handle(GetUpgradeRequestByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.UpgradeRequests
                .Include(ur => ur.User)
                .Include(ur => ur.RequestedPackage)
                .FirstOrDefaultAsync(ur => ur.Id == request.Id, cancellationToken);
        }
    }
}