using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Application.Queries.FreelancerApplications
{
    public class GetFreelancerApplicationByIdQuery : IRequest<FreelancerApplication?>
    {
        public int Id { get; set; }
    }

    public class GetFreelancerApplicationByIdQueryHandler : IRequestHandler<GetFreelancerApplicationByIdQuery, FreelancerApplication?>
    {
        private readonly ApplicationDbContext _context;

        public GetFreelancerApplicationByIdQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FreelancerApplication?> Handle(GetFreelancerApplicationByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.FreelancerApplications
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        }
    }
}
