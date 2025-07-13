using MediatR;
using Microsoft.AspNetCore.Mvc;
using SteadyGrowth.Web.Application.Commands.UpgradeRequests;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpgradeRequestApiController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public UpgradeRequestApiController(IMediator mediator, UserManager<User> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUpgradeRequest([FromBody] CreateUpgradeRequestCommand command)
        {
            var userId = _userManager.GetUserId(User);
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
