using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace SteadyGrowth.Web.Areas.Admin.Pages.KYC
{
    public class ViewDocumentModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ViewDocumentModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public KYCDocument? KYCDocument { get; set; }
        public bool IsImage { get; set; }
        public bool IsPdf { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            KYCDocument = await _context.KYCDocuments
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (KYCDocument == null)
            {
                return NotFound();
            }

            var fileExtension = Path.GetExtension(KYCDocument.FileName).ToLowerInvariant();
            IsImage = fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".gif";
            IsPdf = fileExtension == ".pdf";

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int kycDocumentId, string? adminNotes)
        {
            var kycDocument = await _context.KYCDocuments.FirstOrDefaultAsync(d => d.Id == kycDocumentId);
            if (kycDocument == null)
            {
                return NotFound();
            }

            kycDocument.Status = DocumentStatus.Approved;
            kycDocument.AdminNotes = adminNotes;
            await _context.SaveChangesAsync();

            // Check if all required KYC documents are approved for the user
            await UpdateUserKYCStatus(kycDocument.UserId);

            TempData["SuccessMessage"] = "Document approved successfully.";
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostRejectAsync(int kycDocumentId, string? adminNotes)
        {
            var kycDocument = await _context.KYCDocuments.FirstOrDefaultAsync(d => d.Id == kycDocumentId);
            if (kycDocument == null)
            {
                return NotFound();
            }

            kycDocument.Status = DocumentStatus.Rejected;
            kycDocument.AdminNotes = adminNotes;
            await _context.SaveChangesAsync();

            // Update user's overall KYC status if any document is rejected
            await UpdateUserKYCStatus(kycDocument.UserId);

            TempData["ErrorMessage"] = "Document rejected.";
            return RedirectToPage("Index");
        }

        private async Task UpdateUserKYCStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            var allDocuments = await _context.KYCDocuments.Where(d => d.UserId == userId).ToListAsync();

            bool hasApprovedIdentity = allDocuments.Any(d =>
                (d.DocumentType == DocumentType.NationalID ||
                 d.DocumentType == DocumentType.Passport ||
                 d.DocumentType == DocumentType.DriversLicense) &&
                d.Status == DocumentStatus.Approved);

            bool hasApprovedProofOfAddress = allDocuments.Any(d =>
                (d.DocumentType == DocumentType.UtilityBill ||
                 d.DocumentType == DocumentType.ProofOfAddress) &&
                d.Status == DocumentStatus.Approved);

            if (hasApprovedIdentity && hasApprovedProofOfAddress)
            {
                user.KYCStatus = KYCStatus.Approved;
            }
            else if (allDocuments.Any(d => d.Status == DocumentStatus.Rejected))
            {
                user.KYCStatus = KYCStatus.Rejected;
            }
            else if (allDocuments.Any(d => d.Status == DocumentStatus.Pending))
            {
                user.KYCStatus = KYCStatus.Submitted;
            }
            else
            {
                user.KYCStatus = KYCStatus.NotStarted;
            }
            await _userManager.UpdateAsync(user);
        }
    }
}
