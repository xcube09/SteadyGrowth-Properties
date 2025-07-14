using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Controllers
{
    [ApiController]
    [Route("api/admin/wallet")]
    [Authorize(Roles = "Admin")]
    public class AdminWalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly UserManager<User> _userManager;
		public AdminWalletController(IWalletService walletService, UserManager<User> userManager)
		{
			_walletService = walletService;
			_userManager = userManager;
		}

		public class WalletTransactionRequest
        {
            public string UserId { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty; // "credit" or "debit"
            public decimal Amount { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> PostTransaction([FromBody] WalletTransactionRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.UserId) || req.Amount <= 0 || string.IsNullOrWhiteSpace(req.Type))
                return BadRequest(new { success = false, message = "Invalid input." });
            try
            {
                var adminUserId = _userManager.GetUserId(User);
				if (req.Type == "credit")
                {
                    await _walletService.CreditWalletAsync(req.UserId, req.Amount, req.Description, adminUserId);
                }
                else if (req.Type == "debit")
                {
                    await _walletService.DebitWalletAsync(req.UserId, req.Amount, req.Description);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Invalid type." });
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
} 