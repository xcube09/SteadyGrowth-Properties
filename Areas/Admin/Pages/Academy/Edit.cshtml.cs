using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Commands.Academy;
using SteadyGrowth.Web.Application.Queries.Academy;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Academy
{
    public class EditModel : PageModel
    {
        private readonly IMediator _mediator;

        public EditModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty]
        public UpdateCourseCommand Course { get; set; } = new UpdateCourseCommand();

        public IEnumerable<AcademyPackage> AvailablePackages { get; set; } = new List<AcademyPackage>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var course = await _mediator.Send(new GetCourseByIdQuery { Id = id });

            if (course == null)
            {
                return NotFound();
            }

            Course = new UpdateCourseCommand
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Content = course.Content,
                VideoUrl = course.VideoUrl,
                Duration = course.Duration,
                Order = course.Order,
                IsActive = course.IsActive,
                AcademyPackageId = course.AcademyPackageId
            };

            AvailablePackages = await _mediator.Send(new GetAvailableAcademyPackagesQuery());
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AvailablePackages = await _mediator.Send(new GetAvailableAcademyPackagesQuery());
                return Page();
            }

            var result = await _mediator.Send(Course);

            if (result)
            {
                return RedirectToPage("./Courses");
            }

            ModelState.AddModelError(string.Empty, "Error updating course.");
            AvailablePackages = await _mediator.Send(new GetAvailableAcademyPackagesQuery());
            return Page();
        }
    }
}
