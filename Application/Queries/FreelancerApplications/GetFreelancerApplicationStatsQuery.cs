using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Application.Queries.FreelancerApplications
{
    public class FreelancerApplicationStatsViewModel
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int UnderReview { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }

    public class GetFreelancerApplicationStatsQuery : IRequest<FreelancerApplicationStatsViewModel>
    {
    }

    public class GetFreelancerApplicationStatsQueryHandler : IRequestHandler<GetFreelancerApplicationStatsQuery, FreelancerApplicationStatsViewModel>
    {
        private readonly ApplicationDbContext _context;

        public GetFreelancerApplicationStatsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FreelancerApplicationStatsViewModel> Handle(GetFreelancerApplicationStatsQuery request, CancellationToken cancellationToken)
        {
            var applications = await _context.FreelancerApplications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return new FreelancerApplicationStatsViewModel
            {
                Total = applications.Sum(a => a.Count),
                Pending = applications.FirstOrDefault(a => a.Status == FreelancerApplicationStatus.Pending)?.Count ?? 0,
                UnderReview = applications.FirstOrDefault(a => a.Status == FreelancerApplicationStatus.UnderReview)?.Count ?? 0,
                Approved = applications.FirstOrDefault(a => a.Status == FreelancerApplicationStatus.Approved)?.Count ?? 0,
                Rejected = applications.FirstOrDefault(a => a.Status == FreelancerApplicationStatus.Rejected)?.Count ?? 0
            };
        }
    }
}
