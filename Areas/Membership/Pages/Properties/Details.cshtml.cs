using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Properties;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Properties
{
    /// <summary>
    /// Property details page model for viewing approved property details.
    /// </summary>
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IMediator _mediator;

        public DetailsModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Property? Property { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ViewData["Breadcrumb"] = new List<(string, string)> 
            { 
                ("Properties", "/Membership/Properties/Index"),
                ("Property Details", $"/Membership/Properties/Details/{id}")
            };

            var query = new GetApprovedPropertyDetailsQuery
            {
                PropertyId = id
            };

            Property = await _mediator.Send(query);

            if (Property == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}