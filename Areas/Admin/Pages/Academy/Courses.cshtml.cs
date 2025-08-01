using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SteadyGrowth.Web.Application.Queries.Academy;
using SteadyGrowth.Web.Application.Queries.Properties;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Academy
{
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public class CoursesModel : PageModel
    {
        private readonly IMediator _mediator;

        public CoursesModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public PaginatedList<Course>? Courses { get; set; }
        public IList<AcademyPackage> AvailablePackages { get; set; } = new List<AcademyPackage>();
        public string SearchTerm { get; set; } = string.Empty;
        public int? PackageFilter { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync(string search = "", int? packageFilter = null, int pageIndex = 1)
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("Admin", "/Admin/Properties/Index"), ("Academy", "/Admin/Academy/Courses"), ("Courses", "/Admin/Academy/Courses") };

            SearchTerm = search;
            PackageFilter = packageFilter;
            PageIndex = pageIndex;

            var coursesQuery = new GetAllCoursesQuery
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                PackageFilter = PackageFilter
            };
            Courses = await _mediator.Send(coursesQuery);

            var packagesQuery = new GetAvailableAcademyPackagesQuery();
            AvailablePackages = await _mediator.Send(packagesQuery);
        }

        public class DeleteCourseModel
        {
            public int Id { get; set; }
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] DeleteCourseModel model)
        {
            var result = await _mediator.Send(new SteadyGrowth.Web.Application.Commands.Academy.DeleteCourseCommand { Id = model.Id });
            if (result)
            {
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false, message = "Error deleting course." });
        }

        public async Task<IActionResult> OnPostDeleteSelectedAsync([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new JsonResult(new { success = false, message = "No courses selected." });
            }

            var success = true;
            foreach (var id in ids)
            {
                var result = await _mediator.Send(new SteadyGrowth.Web.Application.Commands.Academy.DeleteCourseCommand { Id = id });
                if (!result)
                {
                    success = false;
                }
            }

            if (success)
            {
                return new JsonResult(new { success = true });
            }

            return new JsonResult(new { success = false, message = "Error deleting some courses." });
        }
    }
}
