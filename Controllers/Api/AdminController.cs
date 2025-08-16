using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IPropertyCommissionService _propertyCommissionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWalletService _walletService;

        public AdminController(IPropertyCommissionService propertyCommissionService, ICurrentUserService currentUserService, IWalletService walletService)
        {
            _propertyCommissionService = propertyCommissionService;
            _currentUserService = currentUserService;
            _walletService = walletService;
        }

        [HttpPost("commission/add")]
        public async Task<IActionResult> AddCommission([FromBody] AddCommissionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request data" });
            }

            try
            {
                var currentUser = await _currentUserService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var commission = await _propertyCommissionService.AddCommissionAsync(
                    request.UserId,
                    request.PropertyId,
                    request.Amount,
                    request.Description,
                    currentUser.Id,
                    request.Reference
                );

                return Ok(new { success = true, commissionId = commission.Id, message = "Commission added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("users/{userId}/wallet-balance")]
        public async Task<IActionResult> GetUserWalletBalance(string userId)
        {
            try
            {
                var balance = await _walletService.GetWalletBalanceAsync(userId);
                return Ok(new { balance, success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class AddCommissionRequest
        {
            public string UserId { get; set; } = string.Empty;
            public int PropertyId { get; set; }
            public decimal Amount { get; set; }
            public string Description { get; set; } = string.Empty;
            public string? Reference { get; set; }
        }
    }
}