using MediatR;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Application.Queries.Wallet
{
    public class GetWalletDetailsQuery : IRequest<WalletDetailsViewModel?>
    {
        public string UserId { get; set; } = string.Empty;

        public class GetWalletDetailsQueryHandler : IRequestHandler<GetWalletDetailsQuery, WalletDetailsViewModel?>
        {
            private readonly IWalletService _walletService;

            public GetWalletDetailsQueryHandler(IWalletService walletService)
            {
                _walletService = walletService;
            }

            public async Task<WalletDetailsViewModel?> Handle(GetWalletDetailsQuery request, CancellationToken cancellationToken)
            {
                var wallet = await _walletService.GetWalletByUserIdAsync(request.UserId);
                if (wallet == null)
                    return null;

                var transactions = await _walletService.GetWalletTransactionsAsync(request.UserId, 1, 10);

                return new WalletDetailsViewModel
                {
                    WalletId = wallet.Id,
                    UserId = wallet.UserId,
                    Balance = wallet.Balance,
                    CreatedAt = wallet.CreatedAt,
                    LastUpdatedAt = wallet.LastUpdatedAt,
                    IsActive = wallet.IsActive,
                    RecentTransactions = transactions.Select(t => new WalletTransactionViewModel
                    {
                        Id = t.Id,
                        TransactionType = t.TransactionType,
                        Status = t.Status,
                        Amount = t.Amount,
                        BalanceBefore = t.BalanceBefore,
                        BalanceAfter = t.BalanceAfter,
                        Description = t.Description,
                        Reference = t.Reference,
                        AdminUserId = t.AdminUserId,
                        CreatedAt = t.CreatedAt,
                        ProcessedAt = t.ProcessedAt
                    }).ToList()
                };
            }
        }
    }

    public class WalletDetailsViewModel
    {
        public int WalletId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<WalletTransactionViewModel> RecentTransactions { get; set; } = new List<WalletTransactionViewModel>();
    }

    public class WalletTransactionViewModel
    {
        public int Id { get; set; }
        public WalletTransactionType TransactionType { get; set; }
        public WalletTransactionStatus Status { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public string? AdminUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
} 