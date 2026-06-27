using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Models.Entities;

public class FreelancerApplication
{
    public int Id { get; set; }

    // Personal Information
    public string FullName { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CurrentResidentialAddress { get; set; } = string.Empty;

    // Professional Background
    public int? YearsOfExperience { get; set; }
    public string? PreviousFirmsOrAgencies { get; set; }
    public string? LanguagesSpoken { get; set; }

    // Areas of Specialization
    public bool SpecResidentialSales { get; set; }
    public bool SpecCommercialProperties { get; set; }
    public bool SpecOffPlanProjectMarketing { get; set; }
    public bool SpecLeadGeneration { get; set; }
    public bool SpecSocialMediaAdvertising { get; set; }
    public bool SpecOthers { get; set; }
    public string? SpecOthersDescription { get; set; }

    // Skills and Competencies
    public bool SkillClientProspecting { get; set; }
    public bool SkillPropertyViewing { get; set; }
    public bool SkillMarketKnowledge { get; set; }

    // Availability and Work Structure
    public FreelancerAvailability Availability { get; set; }
    public bool WillingToWorkOnCommission { get; set; }

    // Portfolio
    public string? SocialMediaLinks { get; set; }
    public string? RecentSalesOrCampaignResults { get; set; }

    // Declaration
    public bool AcceptedTermsAndDeclaration { get; set; }
    public string ApplicantSignature { get; set; } = string.Empty;
    public DateTime SignatureDate { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public FreelancerApplicationStatus Status { get; set; } = FreelancerApplicationStatus.Pending;
    public string? AdminNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
}
