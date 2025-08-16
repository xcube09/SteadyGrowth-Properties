using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Application.Commands.UpgradeRequests;
using System.ComponentModel.DataAnnotations;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Profile
{
    public class UpgradePackageModel : PageModel
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;

        public UpgradePackageModel(ICurrentUserService currentUserService, IMediator mediator, ApplicationDbContext context)
        {
            _currentUserService = currentUserService;
            _mediator = mediator;
            _context = context;
        }

        public string CurrentPackageName { get; set; } = "Basic Package";
        public IList<AcademyPackage> AvailablePackages { get; set; } = new List<AcademyPackage>();
        public UpgradeRequest? PendingRequest { get; set; }
        public bool IsPremiumUser { get; set; }
        
        [BindProperty, Required, StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [BindProperty, StringLength(500)]
        public string? PaymentDetails { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("My Profile", "/Membership/Profile/Index"), ("Upgrade Package", "/Membership/Profile/UpgradePackage") };

            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Identity/Login");
            }

            var user = await _currentUserService.GetCurrentUserWithDetailsAsync(includePackage: true);
            if (user != null && user.AcademyPackage != null)
            {
                CurrentPackageName = user.AcademyPackage.Name;
            }
            
            // Check if user is already on Premium Package
            IsPremiumUser = CurrentPackageName == "Premium Package";

            // Check for pending upgrade request
            PendingRequest = await _context.UpgradeRequests
                .Include(ur => ur.RequestedPackage)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.Status == UpgradeRequestStatus.Pending);

            var query = new GetAvailableAcademyPackagesQuery();
            var allPackages = await _mediator.Send(query);

            // Debug: Log all packages for troubleshooting
            Console.WriteLine($"Current Package: {CurrentPackageName}");
            Console.WriteLine($"Total packages found: {allPackages.Count}");
            foreach (var pkg in allPackages)
            {
                Console.WriteLine($"Package: {pkg.Name}, Price: {pkg.Price:C}, Active: {pkg.IsActive}");
            }

            // Filter out the current package from available packages
            AvailablePackages = allPackages.Where(p => p.Name != CurrentPackageName).ToList();
            
            Console.WriteLine($"Available packages after filtering: {AvailablePackages.Count}");

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitRequestAsync(int packageId)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Identity/Login");
            }

            // Reload user data to check current package
            await OnGetAsync();
            
            // Prevent premium users from submitting requests
            if (IsPremiumUser)
            {
                ModelState.AddModelError(string.Empty, "You are already on the Premium Package - the highest tier available.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if user already has a pending request
            var existingRequest = await _context.UpgradeRequests
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.Status == UpgradeRequestStatus.Pending);

            if (existingRequest != null)
            {
                ModelState.AddModelError(string.Empty, "You already have a pending upgrade request. Please wait for admin approval.");
                await OnGetAsync(); // Reload data
                return Page();
            }

            var command = new CreateUpgradeRequestCommand
            {
                UserId = userId,
                RequestedPackageId = packageId,
                PaymentMethod = PaymentMethod,
                PaymentDetails = PaymentDetails
            };

            try
            {
                var result = await _mediator.Send(command);
                TempData["SuccessMessage"] = "Your upgrade request has been submitted successfully. An admin will review it shortly.";
                return RedirectToPage("./UpgradePackage");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Failed to submit upgrade request. Please try again.");
                await OnGetAsync(); // Reload data
                return Page();
            }
        }
    }

    public class GetAvailableAcademyPackagesQuery : IRequest<IList<AcademyPackage>>
    {
        public class GetAvailableAcademyPackagesQueryHandler : IRequestHandler<GetAvailableAcademyPackagesQuery, IList<AcademyPackage>>
        {
            private readonly ApplicationDbContext _context;

            public GetAvailableAcademyPackagesQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<IList<AcademyPackage>> Handle(GetAvailableAcademyPackagesQuery request, CancellationToken cancellationToken)
            {
                return await _context.AcademyPackages.Where(p => p.IsActive).ToListAsync(cancellationToken);
            }
        }
    }

}
