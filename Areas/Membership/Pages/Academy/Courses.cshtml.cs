using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Academy;
using SteadyGrowth.Web.Models.DTOs;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Threading.Tasks;
using SteadyGrowth.Web.Application.Commands.Academy;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Academy
{
    [Authorize(Policy = "KYCVerified")]
    public class CoursesModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public CoursesModel(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public PaginatedResultDto<Course>? Courses { get; set; }
        public Dictionary<int, CourseProgress?> UserCourseProgress { get; set; } = new Dictionary<int, CourseProgress?>();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 9; // Display 9 courses per page

        public async Task OnGetAsync(int pageIndex = 1)
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("Academy", "/Membership/Academy/Courses") };

            PageIndex = pageIndex;

            var user = await _currentUserService.GetCurrentUserWithDetailsAsync(includePackage: true);
            bool isPremiumMember = user?.AcademyPackageId != null;

            var query = new GetPaginatedCoursesQuery
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                IsPremiumMember = isPremiumMember
            };

            Courses = await _mediator.Send(query);

            if (user != null && Courses != null)
            {
                var userProgressQuery = new GetUserCourseProgressQuery { UserId = user.Id };
                var progressList = await _mediator.Send(userProgressQuery);
                UserCourseProgress = progressList.ToDictionary(p => p.CourseId, p => p);
            }
        }

        public async Task<IActionResult> OnPostStartCourseAsync(int courseId)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToPage("/Identity/Login");
            }

            var command = new StartCourseCommand
            {
                UserId = user.Id,
                CourseId = courseId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                // Optionally, add a success message
            }
            else
            {
                // Optionally, add an error message (e.g., course already started)
            }

            return RedirectToPage();
        }
    }
}
