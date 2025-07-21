using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;

namespace SteadyGrowth.Web.Application.Queries.Wallet
{
    public class GetUserWithdrawalRequestsQuery : IRequest<List<WithdrawalRequest>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class GetUserWithdrawalRequestsQueryHandler : IRequestHandler<GetUserWithdrawalRequestsQuery, List<WithdrawalRequest>>
    {
        private readonly ApplicationDbContext _context;

        public GetUserWithdrawalRequestsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WithdrawalRequest>> Handle(GetUserWithdrawalRequestsQuery request, CancellationToken cancellationToken)
        {
            return await _context.WithdrawalRequests
                .Where(wr => wr.UserId == request.UserId)
                .OrderByDescending(wr => wr.RequestedDate)
                .ToListAsync(cancellationToken);
        }
    }
}
