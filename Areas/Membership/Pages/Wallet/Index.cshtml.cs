using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Application.Queries.Wallet;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Wallet
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;
        private readonly IWalletService _walletService;

        public IndexModel(IMediator mediator, UserManager<User> userManager, IWalletService walletService)
        {
            _mediator = mediator;
            _userManager = userManager;
            _walletService = walletService;
        }

        public WalletDetailsViewModel? WalletDetails { get; set; }
        public decimal CurrentBalance { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public async Task<IActionResult> OnGetAsync(int pageIndex = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login", new { area = "Identity" });
            }

            ViewData["Breadcrumb"] = new List<(string, string)> { ("Dashboard", "/Membership/Dashboard/Index"), ("Wallet", "/Membership/Wallet/Index") };

            PageIndex = pageIndex;

            // Get wallet details
            var query = new GetWalletDetailsQuery { UserId = userId };
            WalletDetails = await _mediator.Send(query);

            if (WalletDetails == null)
            {
                try
                {
                    // Create wallet if it doesn't exist
                    await _walletService.GetOrCreateWalletAsync(userId);
                    WalletDetails = await _mediator.Send(query);
                }
                catch (InvalidOperationException)
                {
                    // User doesn't exist, redirect to login
                    return RedirectToPage("/Identity/Login");
                }
            }

            CurrentBalance = await _walletService.GetWalletBalanceAsync(userId);

            return Page();
        }
    }
} 