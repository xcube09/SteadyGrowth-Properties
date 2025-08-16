using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Academy;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteadyGrowth.Web.Application.Commands.Academy;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Academy
{
    [Authorize(Policy = "KYCVerified")]
    public class MyProgressModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public MyProgressModel(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public List<CourseProgress> CourseProgresses { get; set; } = new List<CourseProgress>();
        public double OverallProgressPercentage { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("Academy", "/Membership/Academy/MyProgress") };

            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null)
            {
                // Handle unauthenticated user, maybe redirect to login
                return;
            }

            var query = new GetUserCourseProgressQuery { UserId = user.Id };
            CourseProgresses = await _mediator.Send(query);

            CalculateOverallProgress();
        }

        public async Task<IActionResult> OnPostMarkLessonCompleteAsync(int courseId)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToPage("/Identity/Login");
            }

            var command = new MarkLessonCompleteCommand
            {
                UserId = user.Id,
                CourseId = courseId
            };

            var result = await _mediator.Send(command);

            // Reload data after update
            await OnGetAsync();
            return Page();
        }

        private void CalculateOverallProgress()
        {
            if (!CourseProgresses.Any())
            {
                OverallProgressPercentage = 0;
                return;
            }

            int totalCompletedLessons = CourseProgresses.Sum(cp => cp.CompletedLessonsCount);
            int totalPossibleLessons = CourseProgresses.Sum(cp => cp.Course.TotalLessons);

            if (totalPossibleLessons > 0)
            {
                OverallProgressPercentage = (double)totalCompletedLessons / totalPossibleLessons * 100;
            }
            else
            {
                OverallProgressPercentage = 0;
            }
        }
    }
}
