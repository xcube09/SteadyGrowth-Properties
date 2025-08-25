using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Services.Implementations
{
    /// <summary>
    /// Academy package management service implementation
    /// </summary>
    public class AcademyPackageService : IAcademyPackageService
    {
        private readonly ApplicationDbContext _context;

        public AcademyPackageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AcademyPackage>> GetAllPackagesAsync()
        {
            return await _context.AcademyPackages
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<AcademyPackage?> GetPackageByIdAsync(int id)
        {
            return await _context.AcademyPackages
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> UpdatePackagePriceAsync(int id, decimal newPrice)
        {
            var package = await _context.AcademyPackages.FindAsync(id);
            if (package == null)
                return false;

            package.Price = newPrice;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TogglePackageActiveStatusAsync(int id)
        {
            var package = await _context.AcademyPackages.FindAsync(id);
            if (package == null)
                return false;

            package.IsActive = !package.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AcademyPackage> CreatePackageAsync(AcademyPackage package)
        {
            package.CreatedAt = DateTime.UtcNow;
            _context.AcademyPackages.Add(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<IEnumerable<(AcademyPackage Package, int UserCount)>> GetPackagesWithUserCountAsync()
        {
            var packages = await _context.AcademyPackages
                .Select(p => new
                {
                    Package = p,
                    UserCount = p.Users.Count(u => u.AcademyPackageId == p.Id)
                })
                .OrderBy(x => x.Package.Name)
                .ToListAsync();

            return packages.Select(x => (x.Package, x.UserCount));
        }
    }
}