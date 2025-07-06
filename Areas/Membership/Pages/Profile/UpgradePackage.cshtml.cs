using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Profile
{
    public class UpgradePackageModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;

        public UpgradePackageModel(UserManager<User> userManager, IMediator mediator)
        {
            _userManager = userManager;
            _mediator = mediator;
        }

        public string CurrentPackageName { get; set; } = "Basic Package";
        public IList<AcademyPackage> AvailablePackages { get; set; } = new List<AcademyPackage>();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("My Profile", "/Membership/Profile/Index"), ("Upgrade Package", "/Membership/Profile/UpgradePackage") };

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.AcademyPackage != null)
            {
                CurrentPackageName = user.AcademyPackage.Name;
            }

            var query = new GetAvailableAcademyPackagesQuery();
            AvailablePackages = await _mediator.Send(query);

            // Filter out the current package from available packages
            AvailablePackages = AvailablePackages.Where(p => p.Name != CurrentPackageName).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostUpgradeAsync(int packageId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            var command = new UpgradeUserPackageCommand
            {
                UserId = userId,
                PackageId = packageId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                // Optionally, add a success message
                return RedirectToPage("./UpgradePackage");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Failed to upgrade package.");
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

    public class UpgradeUserPackageCommand : IRequest<bool>
    {
        public string UserId { get; set; }
        public int PackageId { get; set; }

        public class UpgradeUserPackageCommandHandler : IRequestHandler<UpgradeUserPackageCommand, bool>
        {
            private readonly UserManager<User> _userManager;
            private readonly ApplicationDbContext _context;

            public UpgradeUserPackageCommandHandler(UserManager<User> userManager, ApplicationDbContext context)
            {
                _userManager = userManager;
                _context = context;
            }

            public async Task<bool> Handle(UpgradeUserPackageCommand request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return false;
                }

                var package = await _context.AcademyPackages.FindAsync(request.PackageId);
                if (package == null)
                {
                    return false;
                }

                user.AcademyPackageId = package.Id;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
        }
    }
}
