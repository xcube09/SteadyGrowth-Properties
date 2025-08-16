using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;
using SteadyGrowth.Web.Services.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using SteadyGrowth.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Profile
{
    public class KYCModel : PageModel
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KYCModel(ICurrentUserService currentUserService, IUserService userService, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _currentUserService = currentUserService;
            _userService = userService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public User AppUser { get; set; }
        public List<KYCDocument> KYCDocuments { get; set; } = new List<KYCDocument>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Identity/Login");
            }

            AppUser = await _userService.GetUserByIdAsync(userId);
            if (AppUser == null)
            {
                return NotFound();
            }

            KYCDocuments = await _context.KYCDocuments.Where(d => d.UserId == userId).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUploadDocumentAsync(IFormFile document, DocumentType documentType)
        {
            var user = await _currentUserService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToPage("/Identity/Login");
            }

            if (document != null && document.Length > 0)
            {
                // Save the file
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "kycdocuments");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + document.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(fileStream);
                }

                // Create KYCDocument entry
                var kycDocument = new KYCDocument
                {
                    UserId = user.Id,
                    DocumentType = documentType,
                    FileName = "/kycdocuments/" + uniqueFileName,
                    UploadDate = DateTime.UtcNow,
                    Status = DocumentStatus.Pending
                };

                _context.KYCDocuments.Add(kycDocument);

                // Update user's overall KYC status to Submitted when uploading new documents
                if (user.KYCStatus == KYCStatus.NotStarted || user.KYCStatus == KYCStatus.Rejected)
                {
                    user.KYCStatus = KYCStatus.Submitted;
                    _context.Users.Update(user);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Document uploaded successfully. It will be reviewed shortly.";
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, "Please select a document to upload.");
            await OnGetAsync();
            return Page();
        }
    }
}
