using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces;

/// <summary>
/// Service interface for property vetting and approval workflow.
/// </summary>
public interface IVettingService
{
    /// <summary>
    /// Submits a property for vetting.
    /// </summary>
    Task<bool> SubmitForVettingAsync(int propertyId);

    /// <summary>
    /// Approves a property and logs the action.
    /// </summary>
    Task<bool> ApprovePropertyAsync(int propertyId, string adminId, string? notes = null);

    /// <summary>
    /// Rejects a property and logs the action.
    /// </summary>
    Task<bool> RejectPropertyAsync(int propertyId, string adminId, string notes);

    /// <summary>
    /// Gets the vetting history for a property.
    /// </summary>
    Task<IEnumerable<VettingLog>> GetVettingHistoryAsync(int propertyId);

    /// <summary>
    /// Gets all properties currently awaiting vetting.
    /// </summary>
    Task<IEnumerable<Property>> GetPropertiesForVettingAsync();
}
