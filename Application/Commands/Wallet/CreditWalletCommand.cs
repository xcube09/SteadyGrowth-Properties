using MediatR;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Application.Commands.Wallet
{
    public class CreditWalletCommand : IRequest<WalletTransaction>
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        public string? AdminUserId { get; set; }

        [StringLength(500)]
        public string? Reference { get; set; }

        public class CreditWalletCommandHandler : IRequestHandler<CreditWalletCommand, WalletTransaction>
        {
            private readonly IWalletService _walletService;

            public CreditWalletCommandHandler(IWalletService walletService)
            {
                _walletService = walletService;
            }

            public async Task<WalletTransaction> Handle(CreditWalletCommand request, CancellationToken cancellationToken)
            {
                return await _walletService.CreditWalletAsync(
                    request.UserId,
                    request.Amount,
                    request.Description,
                    request.AdminUserId,
                    request.Reference);
            }
        }
    }
} 