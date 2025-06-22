using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Models.ViewModels;

/// <summary>
/// ViewModel for property search and listing.
/// </summary>
public class PropertySearchViewModel
{
    /// <summary>
    /// Search term for property title or description.
    /// </summary>
    [Display(Name = "Search Term")]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Property type filter.
    /// </summary>
    [Display(Name = "Property Type")]
    public PropertyType? PropertyType { get; set; }

    /// <summary>
    /// Minimum price filter.
    /// </summary>
    [Display(Name = "Minimum Price")]
    [Range(0, 999999999)]
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Maximum price filter.
    /// </summary>
    [Display(Name = "Maximum Price")]
    [Range(0, 999999999)]
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Location filter.
    /// </summary>
    [Display(Name = "Location")]
    public string? Location { get; set; }

    /// <summary>
    /// Properties matching the search.
    /// </summary>
    [Display(Name = "Properties")]
    public IEnumerable<Property> Properties { get; set; }

    /// <summary>
    /// Total number of properties found.
    /// </summary>
    [Display(Name = "Total Count")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number.
    /// </summary>
    [Display(Name = "Current Page")]
    public int CurrentPage { get; set; }

    /// <summary>
    /// Number of properties per page.
    /// </summary>
    [Display(Name = "Page Size")]
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    [Display(Name = "Total Pages")]
    public int TotalPages { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertySearchViewModel"/> class.
    /// </summary>
    public PropertySearchViewModel()
    {
        Properties = Enumerable.Empty<Property>();
        CurrentPage = 1;
        PageSize = 20;
    }
}
