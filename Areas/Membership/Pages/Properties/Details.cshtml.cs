using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
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
        private readonly ICurrencyService _currencyService;

        public DetailsModel(IMediator mediator, ICurrencyService currencyService)
        {
            _mediator = mediator;
            _currencyService = currencyService;
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

        public async Task<string> GetFormattedPriceWithUSDAsync(Property property)
        {
            return $"₦{property.Price:N2}";
        }
    }
}