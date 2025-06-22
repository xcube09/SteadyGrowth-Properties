using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces
{
    /// <summary>
    /// Property management service interface
    /// </summary>
    public interface IPropertyService
    {
        Task<IEnumerable<Property>> GetApprovedPropertiesAsync(int page = 1, int pageSize = 20);
        Task<Property?> GetPropertyByIdAsync(int id);
        Task<Property> CreatePropertyAsync(Property property, string userId);
        Task<bool> UpdatePropertyAsync(Property property);
        Task<IEnumerable<Property>> GetUserPropertiesAsync(string userId);
        // ADMIN: Get all properties (any status, any user)
        Task<IEnumerable<Property>> GetAllPropertiesAsync();
    }
}
