using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Common;

namespace SteadyGrowth.Web.Application.Commands.Wallet
{
    public class CreateWithdrawalRequestCommand : IRequest<CommandResult>
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(200)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string AccountName { get; set; } = string.Empty;
    }

    public class CreateWithdrawalRequestCommandHandler : IRequestHandler<CreateWithdrawalRequestCommand, CommandResult>
    {
        private readonly ApplicationDbContext _context;
        private readonly IWalletService _walletService;

        public CreateWithdrawalRequestCommandHandler(ApplicationDbContext context, IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        public async Task<CommandResult> Handle(CreateWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            var userWallet = await _walletService.GetWalletByUserIdAsync(request.UserId);
            if (userWallet == null || userWallet.Balance < request.Amount)
            {
                return CommandResult.Fail("Insufficient funds in your wallet.");
            }

            // Create a debit transaction for the wallet withdrawal
            var debitResult = await _walletService.DebitWalletAsync(request.UserId, request.Amount, "Withdrawal Request");
            if (!debitResult.IsSuccess)
            {
                return CommandResult.Fail("Failed to debit wallet. Please try again.");
            }

            var withdrawalRequest = new WithdrawalRequest
            {
                UserId = request.UserId,
                Amount = request.Amount,
                BankName = request.BankName,
                AccountNumber = request.AccountNumber,
                AccountName = request.AccountName,
                Status = WithdrawalStatus.Pending,
                RequestedDate = DateTime.UtcNow
            };

            _context.WithdrawalRequests.Add(withdrawalRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Success("Withdrawal request submitted successfully.");
        }
    }
}
