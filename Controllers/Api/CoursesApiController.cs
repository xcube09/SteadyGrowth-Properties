using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteadyGrowth.Web.Application.Commands.Academy;
using SteadyGrowth.Web.Application.Queries.Academy;
using SteadyGrowth.Web.Application.ViewModels;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesApiController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CoursesApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCourse(int id)
        {
            try
            {
                var courseWithSegments = await _mediator.Send(new GetCourseWithSegmentsQuery { CourseId = id });

                if (courseWithSegments == null)
                {
                    return NotFound(new { message = "Course not found" });
                }

                var course = courseWithSegments.Course;
                var segments = courseWithSegments.Segments;

                var response = new
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description ?? "",
                    Duration = course.Duration,
                    Order = course.Order,
                    IsActive = course.IsActive,
                    AcademyPackageId = course.AcademyPackageId,
                    Segments = segments.Select((segment, index) => new
                    {
                        Id = segment.Id,
                        Title = segment.Title ?? "",
                        Content = segment.Content ?? "",
                        VideoUrl = segment.VideoUrl ?? "",
                        Order = segment.Order,
                        IsActive = segment.IsActive
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the course", error = ex.Message });
            }
        }

        [HttpGet("packages")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPackages()
        {
            try
            {
                var packages = await _mediator.Send(new GetAvailableAcademyPackagesQuery());
                var response = packages.Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving packages", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseApiRequest request)
        {
            try
            {
                var command = new CreateCourseCommand
                {
                    Title = request.Title,
                    Description = request.Description,
                    Duration = request.Duration,
                    Order = request.Order,
                    IsActive = request.IsActive,
                    AcademyPackageId = request.AcademyPackageId,
                    Segments = request.Segments.Select(s => new CourseSegmentCreateViewModel
                    {
                        Title = s.Title,
                        Content = s.Content,
                        VideoUrl = s.VideoUrl,
                        Order = s.Order,
                        IsActive = s.IsActive
                    }).ToList()
                };

                var result = await _mediator.Send(command);

                if (result != null)
                {
                    return Ok(new { message = "Course created successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to create course" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the course", error = ex.Message });
            }
        }

        [HttpPost("segments/{segmentId}/complete")]
        [Authorize] // Any authenticated user can mark segments complete
        public async Task<IActionResult> MarkSegmentComplete(int segmentId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var command = new MarkSegmentCompleteCommand
                {
                    UserId = userId,
                    CourseSegmentId = segmentId
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new { message = "Segment marked as completed successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to mark segment as completed" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while marking segment as completed", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseApiRequest request)
        {
            try
            {
                if (id != request.Id)
                {
                    return BadRequest(new { message = "Course ID mismatch" });
                }

                var command = new UpdateCourseCommand
                {
                    Id = request.Id,
                    Title = request.Title,
                    Description = request.Description,
                    Duration = request.Duration,
                    Order = request.Order,
                    IsActive = request.IsActive,
                    AcademyPackageId = request.AcademyPackageId,
                    Segments = request.Segments.Select(s => new CourseSegmentEditViewModel
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Content = s.Content,
                        VideoUrl = s.VideoUrl,
                        Order = s.Order,
                        IsActive = s.IsActive,
                        IsDeleted = s.IsDeleted
                    }).ToList()
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    return Ok(new { message = "Course updated successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to update course" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the course", error = ex.Message });
            }
        }
    }

    public class CreateCourseApiRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public int? AcademyPackageId { get; set; }
        public List<CourseSegmentCreateApiModel> Segments { get; set; } = new();
    }

    public class UpdateCourseApiRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public int? AcademyPackageId { get; set; }
        public List<CourseSegmentApiModel> Segments { get; set; } = new();
    }

    public class CourseSegmentCreateApiModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CourseSegmentApiModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}