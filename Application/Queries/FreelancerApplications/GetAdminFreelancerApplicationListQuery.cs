using MediatR;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Application.Queries.Properties;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Application.Queries.FreelancerApplications
{
    public class FreelancerApplicationListItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public FreelancerAvailability Availability { get; set; }
        public FreelancerApplicationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetAdminFreelancerApplicationListQuery : IRequest<PaginatedList<FreelancerApplicationListItemViewModel>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; } = string.Empty;
        public FreelancerApplicationStatus? StatusFilter { get; set; }
    }

    public class GetAdminFreelancerApplicationListQueryHandler : IRequestHandler<GetAdminFreelancerApplicationListQuery, PaginatedList<FreelancerApplicationListItemViewModel>>
    {
        private readonly ApplicationDbContext _context;

        public GetAdminFreelancerApplicationListQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<FreelancerApplicationListItemViewModel>> Handle(GetAdminFreelancerApplicationListQuery request, CancellationToken cancellationToken)
        {
            var query = _context.FreelancerApplications.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(a => a.FullName.ToLower().Contains(searchTerm) ||
                                         a.Email.ToLower().Contains(searchTerm));
            }

            if (request.StatusFilter.HasValue)
            {
                query = query.Where(a => a.Status == request.StatusFilter.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var applications = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new FreelancerApplicationListItemViewModel
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Email = a.Email,
                    PhoneNumber = a.PhoneNumber,
                    Availability = a.Availability,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PaginatedList<FreelancerApplicationListItemViewModel>(applications, totalCount, request.PageIndex, request.PageSize);
        }
    }
}
