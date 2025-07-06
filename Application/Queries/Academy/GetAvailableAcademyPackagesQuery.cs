using MediatR;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Application.Queries.Academy
{
    public class GetAvailableAcademyPackagesQuery : IRequest<IList<AcademyPackage>>
    {
        public class GetAvailableAcademyPackagesQueryHandler : IRequestHandler<GetAvailableAcademyPackagesQuery, IList<AcademyPackage>>
        {
            private readonly ApplicationDbContext _context;

            public GetAvailableAcademyPackagesQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<IList<AcademyPackage>> Handle(GetAvailableAcademyPackagesQuery request, CancellationToken cancellationToken)
            {
                return await _context.AcademyPackages.Where(p => p.IsActive).ToListAsync(cancellationToken);
            }
        }
    }
}
