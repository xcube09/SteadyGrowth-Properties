using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Properties;
using SteadyGrowth.Web.Models.Entities;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Pages
{
    //[Authorize(Policy = "KYCVerified")]
    public class ListingsModel : PageModel
    {
        private readonly IMediator _mediator;

        public ListingsModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public PaginatedList<Property>? Properties { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 9; // Display 9 properties per page

        public async Task OnGetAsync(int pageIndex = 1)
        {
            PageIndex = pageIndex;

            var query = new GetApprovedPropertyListingQuery
            {
                PageIndex = PageIndex,
                PageSize = PageSize
            };

            Properties = await _mediator.Send(query);
        }
    }
}