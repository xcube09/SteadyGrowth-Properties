using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Queries.FreelancerApplications;
using SteadyGrowth.Web.Application.Queries.Properties;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Areas.Admin.Pages.Freelancers
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public PaginatedList<FreelancerApplicationListItemViewModel>? Applications { get; set; }
        public FreelancerApplicationStatsViewModel? Stats { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public FreelancerApplicationStatus? StatusFilter { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync(string search = "", FreelancerApplicationStatus? status = null, int pageIndex = 1)
        {
            ViewData["Breadcrumb"] = new List<(string, string)> { ("Admin", "/Admin"), ("Freelancers", "/Admin/Freelancers/Index") };

            SearchTerm = search;
            StatusFilter = status;
            PageIndex = pageIndex;

            var listQuery = new GetAdminFreelancerApplicationListQuery
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                StatusFilter = StatusFilter
            };

            var statsQuery = new GetFreelancerApplicationStatsQuery();

            Applications = await _mediator.Send(listQuery);
            Stats = await _mediator.Send(statsQuery);

            return Page();
        }
    }
}
