using MediatR;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.DTOs;

namespace SteadyGrowth.Web.Application.Queries.Academy
{
    public class GetPaginatedCoursesQuery : IRequest<PaginatedResultDto<Course>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IsPremiumMember { get; set; }
    }
}
