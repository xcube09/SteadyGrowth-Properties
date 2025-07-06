using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.ViewModels;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Application.Queries.Properties;

namespace SteadyGrowth.Web.Application.Queries.Users
{
    public class GetUserListingQuery : IRequest<PaginatedList<UserAdminViewModel>>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;

        public class GetUserListingQueryHandler : IRequestHandler<GetUserListingQuery, PaginatedList<UserAdminViewModel>>
        {
            private readonly ApplicationDbContext _context;

            public GetUserListingQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<PaginatedList<UserAdminViewModel>> Handle(GetUserListingQuery request, CancellationToken cancellationToken)
            {
                var query = _context.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(u => u.FirstName.Contains(request.SearchTerm) ||
                                             u.LastName.Contains(request.SearchTerm) ||
                                             u.Email.Contains(request.SearchTerm) ||
                                             u.Id.Contains(request.SearchTerm));
                }

                if (!string.IsNullOrWhiteSpace(request.StatusFilter))
                {
                    if (request.StatusFilter.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTime.UtcNow);
                    }
                    else if (request.StatusFilter.Equals("Suspended", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow);
                    }
                    // Assuming 'Pending' status is not directly mapped to LockoutEnd, 
                    // you might need another property on User entity or a different logic.
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var users = await query.Skip((request.PageIndex - 1) * request.PageSize)
                                     .Take(request.PageSize)
                                     .Select(u => new UserAdminViewModel
                                     {
                                         Id = u.Id,
                                         FullName = u.FirstName + " " + u.LastName,
                                         Email = u.Email!,
                                         Status = u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow ? "Suspended" : "Active",
                                         Role = "User", // Placeholder, actual role fetching might be more complex
                                         RegisteredAt = u.CreatedAt
                                     })
                                     .ToListAsync(cancellationToken);

                return new PaginatedList<UserAdminViewModel>(users, totalCount, request.PageIndex, request.PageSize);
            }
        }
    }
}
