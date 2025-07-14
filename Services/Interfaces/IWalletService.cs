using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces
{
    /// <summary>
    /// Wallet management service interface
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// Get or create a wallet for a user
        /// </summary>
        Task<Wallet> GetOrCreateWalletAsync(string userId);

        /// <summary>
        /// Get wallet by user ID
        /// </summary>
        Task<Wallet?> GetWalletByUserIdAsync(string userId);

        /// <summary>
        /// Get wallet transactions for a user
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetWalletTransactionsAsync(string userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Credit a user's wallet (admin operation)
        /// </summary>
        Task<WalletTransaction> CreditWalletAsync(string userId, decimal amount, string description, string? adminUserId = null, string? reference = null);

        /// <summary>
        /// Debit a user's wallet
        /// </summary>
        Task<WalletTransaction> DebitWalletAsync(string userId, decimal amount, string description, string? reference = null);

        /// <summary>
        /// Add bonus to a user's wallet
        /// </summary>
        Task<WalletTransaction> AddBonusAsync(string userId, decimal amount, string description, string? reference = null);

        /// <summary>
        /// Add commission to a user's wallet
        /// </summary>
        Task<WalletTransaction> AddCommissionAsync(string userId, decimal amount, string description, string? reference = null);

        /// <summary>
        /// Process a pending transaction
        /// </summary>
        Task<bool> ProcessTransactionAsync(int transactionId);

        /// <summary>
        /// Cancel a pending transaction
        /// </summary>
        Task<bool> CancelTransactionAsync(int transactionId, string reason);

        /// <summary>
        /// Get wallet balance for a user
        /// </summary>
        Task<decimal> GetWalletBalanceAsync(string userId);

        /// <summary>
        /// Check if user has sufficient balance
        /// </summary>
        Task<bool> HasSufficientBalanceAsync(string userId, decimal amount);

        /// <summary>
        /// Get transaction by ID
        /// </summary>
        Task<WalletTransaction?> GetTransactionByIdAsync(int transactionId);
    }
} 