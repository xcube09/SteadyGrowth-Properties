using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Models.ViewModels;

/// <summary>
/// ViewModel for creating a new property.
/// </summary>
public class PropertyCreateViewModel
{
    /// <summary>
    /// Property title.
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Property description.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Property price.
    /// </summary>
    [Required]
    [Range(1, 999999999)]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Property location.
    /// </summary>
    [Required]
    [MaxLength(500)]
    [Display(Name = "Location")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Property type.
    /// </summary>
    [Required]
    [Display(Name = "Property Type")]
    public PropertyType PropertyType { get; set; }

    /// <summary>
    /// List of property features.
    /// </summary>
    [Display(Name = "Features")]
    public List<string> Features { get; set; }

    /// <summary>
    /// Uploaded image files.
    /// </summary>
    [Display(Name = "Image Files")]
    public List<IFormFile> ImageFiles { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyCreateViewModel"/> class.
    /// </summary>
    public PropertyCreateViewModel()
    {
        Features = new List<string>();
        ImageFiles = new List<IFormFile>();
    }
}
