using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Academy;
using SteadyGrowth.Web.Application.Commands.Academy;
using SteadyGrowth.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Academy
{
    [Authorize(Policy = "KYCVerified")]
    public class ViewCourseModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public ViewCourseModel(IMediator mediator, UserManager<User> userManager, ApplicationDbContext context)
        {
            _mediator = mediator;
            _userManager = userManager;
            _context = context;
        }

        public Course Course { get; set; } = null!;
        public List<CourseSegmentWithProgress> Segments { get; set; } = new();
        public CourseProgress? UserProgress { get; set; }
        public int CurrentSegmentIndex { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync(int courseId, int? segmentIndex = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Login");
            }

            // Get course details
            Course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (Course == null)
            {
                return NotFound();
            }

            // Get segments with progress
            var segmentsQuery = new GetCourseSegmentsWithProgressQuery 
            { 
                CourseId = courseId, 
                UserId = user.Id 
            };
            Segments = await _mediator.Send(segmentsQuery);

            // Get user's overall course progress
            UserProgress = await _context.CourseProgresses
                .FirstOrDefaultAsync(cp => cp.UserId == user.Id && cp.CourseId == courseId);

            // Determine current segment index
            if (segmentIndex.HasValue && segmentIndex.Value >= 0 && segmentIndex.Value < Segments.Count)
            {
                CurrentSegmentIndex = segmentIndex.Value;
            }
            else
            {
                // Find first incomplete segment or default to 0
                var firstIncomplete = Segments.FindIndex(s => !s.IsCompleted);
                CurrentSegmentIndex = firstIncomplete >= 0 ? firstIncomplete : 0;
            }

            ViewData["Breadcrumb"] = new List<(string, string)> 
            { 
                ("Academy", "/Membership/Academy/Courses"), 
                ("Courses", "/Membership/Academy/Courses"),
                (Course.Title, $"/Membership/Academy/ViewCourse?courseId={courseId}")
            };

            return Page();
        }

        public async Task<IActionResult> OnPostMarkSegmentCompleteAsync(int segmentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Login");
            }

            var command = new MarkSegmentCompleteCommand
            {
                UserId = user.Id,
                CourseSegmentId = segmentId
            };

            await _mediator.Send(command);

            return new JsonResult(new { success = true });
        }
    }
}