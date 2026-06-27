using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Commands.FreelancerApplications;
using SteadyGrowth.Web.Application.Queries.FreelancerApplications;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Freelancers
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public DetailsModel(IMediator mediator, UserManager<User> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        public FreelancerApplication? Application { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var query = new GetFreelancerApplicationByIdQuery { Id = id };
            Application = await _mediator.Send(query);

            if (Application == null)
            {
                return NotFound();
            }

            ViewData["Breadcrumb"] = new List<(string, string)>
            {
                ("Admin", "/Admin"),
                ("Freelancers", "/Admin/Freelancers/Index"),
                ("Application Details", $"/Admin/Freelancers/Details/{id}")
            };

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id, string? adminNotes)
        {
            var adminUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized();
            }

            var command = new UpdateFreelancerApplicationStatusCommand
            {
                ApplicationId = id,
                NewStatus = FreelancerApplicationStatus.Approved,
                AdminNotes = adminNotes,
                ReviewedByUserId = adminUserId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                TempData["SuccessMessage"] = "Freelancer application approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve the application. Please try again.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string? adminNotes)
        {
            var adminUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized();
            }

            var command = new UpdateFreelancerApplicationStatusCommand
            {
                ApplicationId = id,
                NewStatus = FreelancerApplicationStatus.Rejected,
                AdminNotes = adminNotes,
                ReviewedByUserId = adminUserId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                TempData["SuccessMessage"] = "Freelancer application rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject the application. Please try again.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostMarkUnderReviewAsync(int id, string? adminNotes)
        {
            var adminUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Unauthorized();
            }

            var command = new UpdateFreelancerApplicationStatusCommand
            {
                ApplicationId = id,
                NewStatus = FreelancerApplicationStatus.UnderReview,
                AdminNotes = adminNotes,
                ReviewedByUserId = adminUserId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                TempData["SuccessMessage"] = "Freelancer application marked as under review.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update the application status. Please try again.";
            }

            return RedirectToPage(new { id });
        }
    }
}
