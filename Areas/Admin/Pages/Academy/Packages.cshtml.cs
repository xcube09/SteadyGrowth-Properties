using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using SteadyGrowth.Web.Application.Commands.Academy;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Academy
{
    [Authorize(Roles = "Admin")]
    [IgnoreAntiforgeryToken(Order = 2000)]
    public class PackagesModel : PageModel
    {
        private readonly IAcademyPackageService _academyPackageService;
        private readonly IMediator _mediator;

        public PackagesModel(IAcademyPackageService academyPackageService, IMediator mediator)
        {
            _academyPackageService = academyPackageService;
            _mediator = mediator;
        }

        public IEnumerable<(AcademyPackage Package, int UserCount)> PackagesWithUserCount { get; set; } = new List<(AcademyPackage, int)>();

        public async Task OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> 
            { 
                ("Admin", "/Admin/Analytics/Dashboard"), 
                ("Academy", "/Admin/Academy/Courses"), 
                ("Packages", "/Admin/Academy/Packages") 
            };

            PackagesWithUserCount = await _academyPackageService.GetPackagesWithUserCountAsync();
        }

        public async Task<IActionResult> OnPostUpdatePriceAsync([FromBody] UpdatePriceRequest request)
        {
            if (!ModelState.IsValid || request.NewPrice < 0)
            {
                return new JsonResult(new { success = false, message = "Invalid price value" });
            }

            try
            {
                var command = new UpdatePackagePriceCommand
                {
                    PackageId = request.PackageId,
                    NewPrice = request.NewPrice
                };

                var result = await _mediator.Send(command);
                
                if (result)
                {
                    return new JsonResult(new { success = true, message = "Price updated successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Package not found or update failed" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "An error occurred while updating the price" });
            }
        }

        public async Task<IActionResult> OnPostToggleStatusAsync([FromBody] ToggleStatusRequest request)
        {
            try
            {
                var command = new TogglePackageStatusCommand
                {
                    PackageId = request.PackageId
                };

                var result = await _mediator.Send(command);
                
                if (result)
                {
                    return new JsonResult(new { success = true, message = "Package status updated successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Package not found or update failed" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "An error occurred while updating the status" });
            }
        }

        public class UpdatePriceRequest
        {
            public int PackageId { get; set; }
            public decimal NewPrice { get; set; }
        }

        public class ToggleStatusRequest
        {
            public int PackageId { get; set; }
        }
    }
}