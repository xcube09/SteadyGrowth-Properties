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
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KYCModel(UserManager<User> userManager, IUserService userService, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _userService = userService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public User AppUser { get; set; }
        public List<KYCDocument> KYCDocuments { get; set; } = new List<KYCDocument>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
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
            var user = await _userManager.GetUserAsync(User);
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

                // Update user's overall KYC status if it's not already submitted or approved
                if (user.KYCStatus == KYCStatus.NotStarted)
                {
                    user.KYCStatus = KYCStatus.Submitted;
                    await _userManager.UpdateAsync(user);
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
