using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Commands.Academy;
using SteadyGrowth.Web.Application.Queries.Academy;
using SteadyGrowth.Web.Application.ViewModels;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.Linq;
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
        
        public List<CourseSegmentEditViewModel> ExistingSegments { get; set; } = new List<CourseSegmentEditViewModel>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Store the course ID for AJAX loading
            Course.Id = id;
            ViewData["CourseId"] = id;
            ViewData["Breadcrumb"] = new List<(string, string)> { ("Admin", "/Admin/Properties/Index"), ("Academy", "/Admin/Academy/Courses"), ("Edit Course", $"/Admin/Academy/Edit/{id}") };
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
