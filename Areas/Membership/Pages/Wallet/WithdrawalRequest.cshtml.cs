using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Commands.Wallet;
using SteadyGrowth.Web.Application.Queries.Wallet;
using SteadyGrowth.Web.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using SteadyGrowth.Web.Common;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Wallet
{
    public class WithdrawalRequestModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public WithdrawalRequestModel(IMediator mediator, UserManager<User> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [BindProperty]
        public CreateWithdrawalRequestCommand Command { get; set; } = new CreateWithdrawalRequestCommand();

        public List<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();

        public async Task OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("Wallet", "/Membership/Wallet/Index"), ("Withdrawal Requests", "/Membership/Wallet/WithdrawalRequest") };

            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                WithdrawalRequests = await _mediator.Send(new GetUserWithdrawalRequestsQuery { UserId = userId });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToPage("/Identity/Login");
            }

            Command.UserId = userId;

            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Re-populate list if validation fails
                return Page();
            }

            var result = await _mediator.Send(Command);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToPage();
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
                await OnGetAsync(); // Re-populate list if submission fails
                return Page();
            }
        }
    }
}
