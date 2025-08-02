using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Commands.Academy;
using SteadyGrowth.Web.Application.Queries.Academy;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using SteadyGrowth.Web.Data;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Academy
{
    public class AddModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AddModel(IMediator mediator, IWebHostEnvironment webHostEnvironment)
        {
            _mediator = mediator;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public CreateCourseCommand? Command { get; set; }

        public IList<AcademyPackage> AvailablePackages { get; set; } = new List<AcademyPackage>();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("Admin", "/Admin/Properties/Index"), ("Academy", "/Admin/Academy/Courses"), ("Add Course", "/Admin/Academy/Add") };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Command == null)
            {
                return Page();
            }

            if (!ModelState.IsValid)
            {
                AvailablePackages = await _mediator.Send(new GetAvailableAcademyPackagesQuery());
                return Page();
            }

            var result = await _mediator.Send(Command);

            if (result != null)
            {
                return RedirectToPage("./Courses");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error adding course.");
                AvailablePackages = await _mediator.Send(new GetAvailableAcademyPackagesQuery());
                return Page();
            }
        }
    }
}