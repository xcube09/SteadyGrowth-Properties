using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Academy.UpgradeRequests
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IUpgradeRequestService _upgradeRequestService;

        public IndexModel(IUpgradeRequestService upgradeRequestService)
        {
            _upgradeRequestService = upgradeRequestService;
        }

        public IList<UpgradeRequest> UpgradeRequests { get; set; } = new List<UpgradeRequest>();
        public UpgradeRequestStats Stats { get; set; } = new UpgradeRequestStats();

        [BindProperty(SupportsGet = true)]
        public UpgradeRequestStatus? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public async Task OnGetAsync()
        {
            ViewData["Breadcrumb"] = new List<(string, string)> 
            { 
                ("Academy", "/Admin/Academy"), 
                ("Upgrade Requests", "/Admin/Academy/UpgradeRequests/Index")
            };

            UpgradeRequests = await _upgradeRequestService.GetUpgradeRequestsAsync(
                StatusFilter, SearchTerm, FromDate, ToDate, PageIndex, PageSize);

            Stats = await _upgradeRequestService.GetUpgradeRequestStatsAsync();
        }

        public IActionResult OnPostClearFilters()
        {
            return RedirectToPage(new { pageIndex = 1 });
        }
    }
}