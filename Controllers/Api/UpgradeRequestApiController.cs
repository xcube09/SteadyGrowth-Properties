using MediatR;
using Microsoft.AspNetCore.Mvc;
using SteadyGrowth.Web.Application.Commands.UpgradeRequests;
using SteadyGrowth.Web.Services.Interfaces;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpgradeRequestApiController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public UpgradeRequestApiController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUpgradeRequest([FromBody] CreateUpgradeRequestCommand command)
        {
            // GetUserId() from ICurrentUserService automatically handles impersonation
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            command.UserId = userId;

            var result = await _mediator.Send(command);

            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }
    }
}
