using MediatR;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;

namespace SteadyGrowth.Web.Application.Queries.Academy
{
    public class GetUserCourseProgressQuery : IRequest<List<CourseProgress>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}
