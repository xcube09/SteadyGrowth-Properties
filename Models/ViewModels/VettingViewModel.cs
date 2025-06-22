using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Models.ViewModels;

/// <summary>
/// ViewModel for property vetting and approval.
/// </summary>
public class VettingViewModel
{
    /// <summary>
    /// The property being vetted.
    /// </summary>
    [Required]
    [Display(Name = "Property")]
    public Property Property { get; set; }

    /// <summary>
    /// Vetting history for the property.
    /// </summary>
    [Display(Name = "Vetting History")]
    public List<VettingLog> VettingHistory { get; set; }

    /// <summary>
    /// Decision to approve or reject.
    /// </summary>
    [Display(Name = "Decision")]
    public VettingAction Decision { get; set; }

    /// <summary>
    /// Optional notes for the vetting decision.
    /// </summary>
    [MaxLength(1000)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates if the property can be approved.
    /// </summary>
    [Display(Name = "Can Approve")]
    public bool CanApprove { get; set; }

    /// <summary>
    /// Indicates if the property can be rejected.
    /// </summary>
    [Display(Name = "Can Reject")]
    public bool CanReject { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VettingViewModel"/> class.
    /// </summary>
    public VettingViewModel()
    {
        Property = new Property();
        VettingHistory = new List<VettingLog>();
    }
}
