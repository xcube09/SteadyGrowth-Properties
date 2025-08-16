using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces
{
    /// <summary>
    /// Property commission management service interface
    /// </summary>
    public interface IPropertyCommissionService
    {
        /// <summary>
        /// Add commission to a user for a property sale
        /// Creates both PropertyCommission record and corresponding WalletTransaction
        /// </summary>
        Task<PropertyCommission> AddCommissionAsync(string userId, int propertyId, decimal amount, string description, string addedByUserId, string? reference = null);
        
        /// <summary>
        /// Get all commissions for a user
        /// </summary>
        Task<IEnumerable<PropertyCommission>> GetUserCommissionsAsync(string userId);
        
        /// <summary>
        /// Get commissions for a specific property
        /// </summary>
        Task<IEnumerable<PropertyCommission>> GetPropertyCommissionsAsync(int propertyId);
        
        /// <summary>
        /// Get commission by ID
        /// </summary>
        Task<PropertyCommission?> GetCommissionByIdAsync(int id);
        
        /// <summary>
        /// Get user's properties through commission earnings (properties they've earned commission on)
        /// </summary>
        Task<IEnumerable<Property>> GetUserCommissionPropertiesAsync(string userId);
        
        /// <summary>
        /// Get total commission earned by user
        /// </summary>
        Task<decimal> GetUserTotalCommissionAsync(string userId);
        
        /// <summary>
        /// Get total commission earned by user for a specific property
        /// </summary>
        Task<decimal> GetUserPropertyCommissionAsync(string userId, int propertyId);
        
        /// <summary>
        /// Search properties for commission assignment (admin use)
        /// </summary>
        Task<IEnumerable<Property>> SearchPropertiesAsync(string searchTerm, int maxResults = 10);
    }
}