using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SteadyGrowth.Web.Application.Queries.Properties;

namespace SteadyGrowth.Web.Areas.Membership.Pages.Properties;

/// <summary>
/// Membership properties listing page model with filtering and pagination.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrencyService _currencyService;

    public IndexModel(IMediator mediator, ICurrencyService currencyService)
    {
        _mediator = mediator;
        _currencyService = currencyService;
    }

    public PaginatedList<Property>? Properties { get; set; }
    public string? TitleFilter { get; set; }
    public string? LocationFilter { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync(string? title, string? location, decimal? minPrice, decimal? maxPrice, int pageIndex = 1)
    {
        ViewData["Breadcrumb"] = new List<(string, string)> { ("Properties", "/Membership/Properties/Index") };
        TitleFilter = title;
        LocationFilter = location;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        PageIndex = pageIndex;

        var query = new GetApprovedPropertyListingQuery
        {
            PageIndex = PageIndex,
            PageSize = PageSize,
            TitleFilter = TitleFilter,
            LocationFilter = LocationFilter,
            MinPrice = MinPrice,
            MaxPrice = MaxPrice
        };

        Properties = await _mediator.Send(query);
    }

    public async Task<string> GetFormattedPriceWithUSDAsync(Property property)
    {
        var currencyCode = property.CurrencyCode ?? "USD";
        var currencySymbol = await _currencyService.GetCurrencySymbolAsync(currencyCode);
        var originalPrice = $"{currencySymbol}{property.Price:N2}";

        if (currencyCode == "USD")
        {
            return originalPrice;
        }

        try
        {
            var usdPrice = await _currencyService.ConvertAmountAsync(property.Price, currencyCode, "USD");
            return $"{originalPrice} (${usdPrice:N2} USD)";
        }
        catch
        {
            return originalPrice;
        }
    }
}
