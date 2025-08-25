using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces
{
    /// <summary>
    /// Academy package management service interface
    /// </summary>
    public interface IAcademyPackageService
    {
        /// <summary>
        /// Get all academy packages
        /// </summary>
        Task<IEnumerable<AcademyPackage>> GetAllPackagesAsync();
        
        /// <summary>
        /// Get academy package by ID
        /// </summary>
        Task<AcademyPackage?> GetPackageByIdAsync(int id);
        
        /// <summary>
        /// Update academy package price
        /// </summary>
        Task<bool> UpdatePackagePriceAsync(int id, decimal newPrice);
        
        /// <summary>
        /// Toggle academy package active status
        /// </summary>
        Task<bool> TogglePackageActiveStatusAsync(int id);
        
        /// <summary>
        /// Create new academy package
        /// </summary>
        Task<AcademyPackage> CreatePackageAsync(AcademyPackage package);
        
        /// <summary>
        /// Get package with user count
        /// </summary>
        Task<IEnumerable<(AcademyPackage Package, int UserCount)>> GetPackagesWithUserCountAsync();
    }
}