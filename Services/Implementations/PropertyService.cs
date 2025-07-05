using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Services.Implementations;

/// <summary>
/// Service for managing properties and related operations.
/// </summary>
public class PropertyService : IPropertyService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PropertyService> _logger;

    public PropertyService(ApplicationDbContext db, ILogger<PropertyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Property>> GetApprovedPropertiesAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var query = _db.Properties.Include(x => x.PropertyImages)
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p => p.Status == PropertyStatus.Approved)
                .OrderByDescending(p => p.CreatedAt)
                .AsSplitQuery();

            var result = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);

            _logger.LogInformation("Fetched {Count} approved properties (Page: {Page}, PageSize: {PageSize})", result.Count, page, pageSize);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching approved properties");
            return Enumerable.Empty<Property>();
        }
    }

    /// <inheritdoc />
    public async Task<Property?> GetPropertyByIdAsync(int id)
    {
        try
        {
            var property = await _db.Properties.Include(x => x.PropertyImages)
				.AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id)
                .ConfigureAwait(false);
            if (property != null)
                _logger.LogInformation("Fetched property with ID {Id}", id);
            else
                _logger.LogInformation("Property with ID {Id} not found", id);
            return property;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching property by ID {Id}", id);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Property> CreatePropertyAsync(Property property, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(property.Title) || string.IsNullOrWhiteSpace(property.Location))
                throw new ArgumentException("Property Title and Location are required.");

            property.UserId = userId;
            property.CreatedAt = DateTime.UtcNow;
            property.Status = PropertyStatus.Draft;

            _db.Properties.Add(property);
            await _db.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Created new property for user {UserId} with ID {PropertyId}", userId, property.Id);
            return property;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdatePropertyAsync(Property property)
    {
        try
        {
            var existing = await _db.Properties.FirstOrDefaultAsync(p => p.Id == property.Id).ConfigureAwait(false);
            if (existing == null)
            {
                _logger.LogInformation("Property with ID {Id} not found for update", property.Id);
                return false;
            }
            // Update fields
            existing.Title = property.Title;
            existing.Description = property.Description;
            existing.Location = property.Location;
            existing.Price = property.Price;
            existing.PropertyType = property.PropertyType;
            existing.Status = property.Status;
            // Removed: existing.UpdatedAt = DateTime.UtcNow;
            // ... update other fields as needed

            await _db.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Updated property with ID {Id}", property.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating property with ID {Id}", property.Id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Property>> GetUserPropertiesAsync(string userId)
    {
        try
        {
            var properties = await _db.Properties
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync()
                .ConfigureAwait(false);
            _logger.LogInformation("Fetched {Count} properties for user {UserId}", properties.Count, userId);
            return properties;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching properties for user {UserId}", userId);
            return Enumerable.Empty<Property>();
        }
    }

    /// <summary>
    /// Updates the status of a property and logs the change.
    /// </summary>
    public async Task<bool> UpdatePropertyStatusAsync(int propertyId, PropertyStatus newStatus, string adminUserId, string? notes = null)
    {
        try
        {
            var property = await _db.Properties.FirstOrDefaultAsync(p => p.Id == propertyId).ConfigureAwait(false);
            if (property == null)
            {
                _logger.LogInformation("Property with ID {Id} not found for status update", propertyId);
                return false;
            }
            var oldStatus = property.Status;
            property.Status = newStatus;
            if (newStatus == PropertyStatus.Approved)
                property.ApprovedAt = DateTime.UtcNow;
            // Removed: property.UpdatedAt = DateTime.UtcNow;

            // Create vetting log
            // FIX: Use _db.Set<VettingLog>() instead of _db.VettingLogs
            var vettingLog = new VettingLog
            {
                PropertyId = propertyId,
                AdminUserId = adminUserId,
                Action = newStatus switch
                {
                    PropertyStatus.Approved => VettingAction.Approved,
                    PropertyStatus.Rejected => VettingAction.Rejected,
                    PropertyStatus.Pending => VettingAction.UnderReview,
                    _ => VettingAction.Submitted
                },
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };
            _db.Set<VettingLog>().Add(vettingLog);
            await _db.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Updated property status for ID {Id} from {OldStatus} to {NewStatus}", propertyId, oldStatus, newStatus);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating property status for ID {Id}", propertyId);
            return false;
        }
    }

    /// <summary>
    /// Searches properties by title, address, or description.
    /// </summary>
    public async Task<IEnumerable<Property>> SearchPropertiesAsync(string? searchTerm, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = _db.Properties
                .AsNoTracking()
                .Include(p => p.User)
                .AsSplitQuery();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    (p.Title != null && p.Title.Contains(searchTerm)) ||
                    (p.Location != null && p.Location.Contains(searchTerm)) ||
                    (p.Description != null && p.Description.Contains(searchTerm)));
            }

            var result = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);

            _logger.LogInformation("Searched properties with term '{SearchTerm}' (Page: {Page}, PageSize: {PageSize}) - Found: {Count}", searchTerm, page, pageSize, result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching properties with term '{SearchTerm}'", searchTerm);
            return Enumerable.Empty<Property>();
        }
    }

    /// <summary>
    /// ADMIN: Gets all properties (any status, any user)
    /// </summary>
    public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
    {
        try
        {
            var properties = await _db.Properties
                .AsNoTracking()
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync()
                .ConfigureAwait(false);
            _logger.LogInformation("Fetched {Count} properties (all statuses, all users)", properties.Count);
            return properties;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all properties for admin");
            return Enumerable.Empty<Property>();
        }
    }
}
