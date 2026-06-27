using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Areas.Identity.Pages;

public class RegistrationPendingModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public RegistrationPendingModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public string Email { get; set; } = string.Empty;
    public string? PackageName { get; set; }
    public decimal PackagePrice { get; set; }
    public bool RegistrationFound { get; set; }

    public async Task<IActionResult> OnGetAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("/Register");
        }

        Email = email;

        // Fetch the pending registration to display package info
        var pendingRegistration = await _context.PendingRegistrations
            .Include(p => p.SelectedPackage)
            .Where(p => p.Email == email && !p.IsDeleted && p.Status == PendingRegistrationStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (pendingRegistration != null)
        {
            RegistrationFound = true;
            PackageName = pendingRegistration.SelectedPackage?.Name;
            PackagePrice = pendingRegistration.SelectedPackage?.Price ?? 0;
        }
        else
        {
            RegistrationFound = false;
        }

        return Page();
    }
}
