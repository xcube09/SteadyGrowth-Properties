using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Services.Implementations
{
    /// <summary>
    /// Property commission management service implementation
    /// </summary>
    public class PropertyCommissionService : IPropertyCommissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWalletService _walletService;

        public PropertyCommissionService(ApplicationDbContext context, IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        public async Task<PropertyCommission> AddCommissionAsync(string userId, int propertyId, decimal amount, string description, string addedByUserId, string? reference = null)
        {
            try
            {
                // Create wallet transaction first (this handles its own transaction)
                var walletTransaction = await _walletService.AddCommissionAsync(userId, amount, description, reference);
                
                // Create property commission record
                var commission = new PropertyCommission
                {
                    UserId = userId,
                    PropertyId = propertyId,
                    CommissionAmount = amount,
                    Description = description,
                    Reference = reference,
                    AddedByUserId = addedByUserId,
                    WalletTransactionId = walletTransaction.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.PropertyCommissions.Add(commission);
                await _context.SaveChangesAsync();
                
                // Load navigation properties for return
                await _context.Entry(commission)
                    .Reference(c => c.User)
                    .LoadAsync();
                await _context.Entry(commission)
                    .Reference(c => c.Property)
                    .LoadAsync();
                await _context.Entry(commission)
                    .Reference(c => c.AddedBy)
                    .LoadAsync();

                return commission;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<PropertyCommission>> GetUserCommissionsAsync(string userId)
        {
            return await _context.PropertyCommissions
                .Include(pc => pc.Property)
                .Include(pc => pc.WalletTransaction)
                .Where(pc => pc.UserId == userId && pc.IsActive)
                .OrderByDescending(pc => pc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PropertyCommission>> GetPropertyCommissionsAsync(int propertyId)
        {
            return await _context.PropertyCommissions
                .Include(pc => pc.User)
                .Include(pc => pc.AddedBy)
                .Include(pc => pc.WalletTransaction)
                .Where(pc => pc.PropertyId == propertyId && pc.IsActive)
                .OrderByDescending(pc => pc.CreatedAt)
                .ToListAsync();
        }

        public async Task<PropertyCommission?> GetCommissionByIdAsync(int id)
        {
            return await _context.PropertyCommissions
                .Include(pc => pc.User)
                .Include(pc => pc.Property)
                .Include(pc => pc.AddedBy)
                .Include(pc => pc.WalletTransaction)
                .FirstOrDefaultAsync(pc => pc.Id == id && pc.IsActive);
        }

        public async Task<IEnumerable<Property>> GetUserCommissionPropertiesAsync(string userId)
        {
            return await _context.PropertyCommissions
                .Include(pc => pc.Property)
                .Where(pc => pc.UserId == userId && pc.IsActive)
                .Select(pc => pc.Property)
                .Distinct()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetUserTotalCommissionAsync(string userId)
        {
            return await _context.PropertyCommissions
                .Where(pc => pc.UserId == userId && pc.IsActive)
                .SumAsync(pc => pc.CommissionAmount);
        }

        public async Task<decimal> GetUserPropertyCommissionAsync(string userId, int propertyId)
        {
            return await _context.PropertyCommissions
                .Where(pc => pc.UserId == userId && pc.PropertyId == propertyId && pc.IsActive)
                .SumAsync(pc => pc.CommissionAmount);
        }

        public async Task<IEnumerable<Property>> SearchPropertiesAsync(string searchTerm, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Property>();

            var normalizedSearchTerm = searchTerm.ToLower().Trim();

            return await _context.Properties
                .Include(p => p.User)
                .Where(p => p.IsActive && 
                           (p.Title.ToLower().Contains(normalizedSearchTerm) ||
                            p.Location.ToLower().Contains(normalizedSearchTerm) ||
                            p.User.FirstName.ToLower().Contains(normalizedSearchTerm) ||
                            p.User.LastName.ToLower().Contains(normalizedSearchTerm)))
                .OrderByDescending(p => p.CreatedAt)
                .Take(maxResults)
                .ToListAsync();
        }
    }
}