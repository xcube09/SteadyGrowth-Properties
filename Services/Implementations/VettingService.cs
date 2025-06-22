using SteadyGrowth.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Implementations
{
    public class VettingService : IVettingService
    {
        private readonly ILogger<VettingService> _logger;
        public VettingService(ILogger<VettingService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SubmitForVettingAsync(int propertyId) => Task.FromResult(false);
        public Task<bool> ApprovePropertyAsync(int propertyId, string adminId, string? notes = null) => Task.FromResult(false);
        public Task<bool> RejectPropertyAsync(int propertyId, string adminId, string notes) => Task.FromResult(false);
        public Task<IEnumerable<VettingLog>> GetVettingHistoryAsync(int propertyId) => Task.FromResult<IEnumerable<VettingLog>>(Array.Empty<VettingLog>());
        public Task<IEnumerable<Property>> GetPropertiesForVettingAsync() => Task.FromResult<IEnumerable<Property>>(Array.Empty<Property>());
    }
}
