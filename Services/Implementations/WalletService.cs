using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Common;
using System.Transactions;

namespace SteadyGrowth.Web.Services.Implementations
{
    /// <summary>
    /// Wallet management service implementation
    /// </summary>
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<WalletService> _logger;

        public WalletService(ApplicationDbContext db, ILogger<WalletService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Wallet> GetOrCreateWalletAsync(string userId)
        {
            try
            {
                // First, verify the user exists
                var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    throw new InvalidOperationException($"User with ID '{userId}' does not exist in the database.");
                }

                var wallet = await _db.Wallets
                    .Include(w => w.User)
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.IsActive);

                if (wallet == null)
                {
                    wallet = new Wallet
                    {
                        UserId = userId,
                        Balance = 0.00m,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _db.Wallets.Add(wallet);
                    await _db.SaveChangesAsync();

                    _logger.LogInformation("Created new wallet for user {UserId} in USD", userId);
                }

                return wallet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating wallet for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Wallet?> GetWalletByUserIdAsync(string userId)
        {
            try
            {
                return await _db.Wallets
                    .Include(w => w.User)
                    .Include(w => w.Transactions.OrderByDescending(t => t.CreatedAt).Take(10))
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet for user {UserId}", userId);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WalletTransaction>> GetWalletTransactionsAsync(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var wallet = await GetWalletByUserIdAsync(userId);
                if (wallet == null)
                    return Enumerable.Empty<WalletTransaction>();

                return await _db.WalletTransactions
                    .Include(t => t.Wallet)
                    .Include(t => t.AdminUser)
                    .Where(t => t.Wallet.UserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet transactions for user {UserId}", userId);
                return Enumerable.Empty<WalletTransaction>();
            }
        }

        /// <inheritdoc />
        public async Task<WalletTransaction> CreditWalletAsync(string userId, decimal amount, string description, string? adminUserId = null, string? reference = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var wallet = await GetOrCreateWalletAsync(userId);
                var balanceBefore = wallet.Balance;
                var balanceAfter = balanceBefore + amount;

                var transaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    TransactionType = WalletTransactionType.Credit,
                    Status = WalletTransactionStatus.Completed,
                    Amount = amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Description = description,
                    Reference = reference,
                    AdminUserId = adminUserId,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                _db.WalletTransactions.Add(transaction);

                // Update wallet balance
                wallet.Balance = balanceAfter;
                wallet.LastUpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                scope.Complete();

                _logger.LogInformation("Credited wallet for user {UserId} with ${Amount} USD. New balance: ${Balance}", 
                    userId, amount, balanceAfter);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crediting wallet for user {UserId} with amount ${Amount} USD", userId, amount);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<CommandResult> DebitWalletAsync(string userId, decimal amount, string description, string? reference = null)
        {
            if (amount <= 0)
                return CommandResult.Fail("Amount must be greater than zero.");

            using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var wallet = await GetOrCreateWalletAsync(userId);
                
                if (wallet.Balance < amount)
                    return CommandResult.Fail($"Insufficient balance. Current balance: {wallet.Balance:N2}, Required: {amount:N2}");

                var balanceBefore = wallet.Balance;
                var balanceAfter = balanceBefore - amount;

                var transaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    TransactionType = WalletTransactionType.Debit,
                    Status = WalletTransactionStatus.Completed,
                    Amount = amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Description = description,
                    Reference = reference,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                _db.WalletTransactions.Add(transaction);

                // Update wallet balance
                wallet.Balance = balanceAfter;
                wallet.LastUpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                scope.Complete();

                _logger.LogInformation("Debited wallet for user {UserId} with amount {Amount}. New balance: {Balance}", 
                    userId, amount, balanceAfter);

                return CommandResult.Success("Wallet debited successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debiting wallet for user {UserId} with amount {Amount}", userId, amount);
                return CommandResult.Fail("An error occurred while debiting the wallet.");
            }
        }

        /// <inheritdoc />
        public async Task<WalletTransaction> AddBonusAsync(string userId, decimal amount, string description, string? reference = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var wallet = await GetOrCreateWalletAsync(userId);
                var balanceBefore = wallet.Balance;
                var balanceAfter = balanceBefore + amount;

                var transaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    TransactionType = WalletTransactionType.Bonus,
                    Status = WalletTransactionStatus.Completed,
                    Amount = amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Description = description,
                    Reference = reference,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                _db.WalletTransactions.Add(transaction);

                // Update wallet balance
                wallet.Balance = balanceAfter;
                wallet.LastUpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                scope.Complete();

                _logger.LogInformation("Added bonus to wallet for user {UserId} with amount {Amount}. New balance: {Balance}", 
                    userId, amount, balanceAfter);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding bonus to wallet for user {UserId} with amount {Amount}", userId, amount);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<WalletTransaction> AddCommissionAsync(string userId, decimal amount, string description, string? reference = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var wallet = await GetOrCreateWalletAsync(userId);
                var balanceBefore = wallet.Balance;
                var balanceAfter = balanceBefore + amount;

                var transaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    TransactionType = WalletTransactionType.Commission,
                    Status = WalletTransactionStatus.Completed,
                    Amount = amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Description = description,
                    Reference = reference,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                _db.WalletTransactions.Add(transaction);

                // Update wallet balance
                wallet.Balance = balanceAfter;
                wallet.LastUpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                scope.Complete();

                _logger.LogInformation("Added commission to wallet for user {UserId} with amount {Amount}. New balance: {Balance}", 
                    userId, amount, balanceAfter);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding commission to wallet for user {UserId} with amount {Amount}", userId, amount);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ProcessTransactionAsync(int transactionId)
        {
            try
            {
                var transaction = await _db.WalletTransactions
                    .Include(t => t.Wallet)
                    .FirstOrDefaultAsync(t => t.Id == transactionId);

                if (transaction == null || transaction.Status != WalletTransactionStatus.Pending)
                    return false;

                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var wallet = transaction.Wallet;
                var balanceBefore = wallet.Balance;
                var balanceAfter = transaction.TransactionType == WalletTransactionType.Debit 
                    ? balanceBefore - transaction.Amount 
                    : balanceBefore + transaction.Amount;

                if (transaction.TransactionType == WalletTransactionType.Debit && balanceAfter < 0)
                {
                    transaction.Status = WalletTransactionStatus.Failed;
                    transaction.Description += " - Insufficient balance";
                }
                else
                {
                    transaction.Status = WalletTransactionStatus.Completed;
                    transaction.BalanceBefore = balanceBefore;
                    transaction.BalanceAfter = balanceAfter;
                    transaction.ProcessedAt = DateTime.UtcNow;

                    // Update wallet balance
                    wallet.Balance = balanceAfter;
                    wallet.LastUpdatedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();
                scope.Complete();

                _logger.LogInformation("Processed transaction {TransactionId} for wallet {WalletId}. Status: {Status}", 
                    transactionId, wallet.Id, transaction.Status);

                return transaction.Status == WalletTransactionStatus.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transaction {TransactionId}", transactionId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> CancelTransactionAsync(int transactionId, string reason)
        {
            try
            {
                var transaction = await _db.WalletTransactions
                    .FirstOrDefaultAsync(t => t.Id == transactionId);

                if (transaction == null || transaction.Status != WalletTransactionStatus.Pending)
                    return false;

                transaction.Status = WalletTransactionStatus.Cancelled;
                transaction.Description += $" - Cancelled: {reason}";

                await _db.SaveChangesAsync();

                _logger.LogInformation("Cancelled transaction {TransactionId}. Reason: {Reason}", transactionId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling transaction {TransactionId}", transactionId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<decimal> GetWalletBalanceAsync(string userId)
        {
            try
            {
                var wallet = await _db.Wallets
                    .Where(w => w.UserId == userId && w.IsActive)
                    .Select(w => w.Balance)
                    .FirstOrDefaultAsync();

                return wallet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet balance for user {UserId}", userId);
                return 0.00m;
            }
        }

        /// <inheritdoc />
        public async Task<bool> HasSufficientBalanceAsync(string userId, decimal amount)
        {
            try
            {
                var balance = await GetWalletBalanceAsync(userId);
                return balance >= amount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking sufficient balance for user {UserId}", userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<WalletTransaction?> GetTransactionByIdAsync(int transactionId)
        {
            try
            {
                return await _db.WalletTransactions
                    .Include(t => t.Wallet)
                    .Include(t => t.AdminUser)
                    .FirstOrDefaultAsync(t => t.Id == transactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction {TransactionId}", transactionId);
                return null;
            }
        }
    }
} 