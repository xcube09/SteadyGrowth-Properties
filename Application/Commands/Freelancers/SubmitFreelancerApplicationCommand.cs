using System.ComponentModel.DataAnnotations;
using MediatR;
using SteadyGrowth.Web.Application.Validation;
using SteadyGrowth.Web.Data;
using SteadyGrowth.Web.Models.Entities;
using SteadyGrowth.Web.Models.Enums;

namespace SteadyGrowth.Web.Application.Commands.Freelancers;

public class SubmitFreelancerApplicationCommand : IRequest<FreelancerApplication>
{
    // Personal Information
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nationality is required")]
    [StringLength(100, ErrorMessage = "Nationality cannot exceed 100 characters")]
    public string Nationality { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters")]
    [Display(Name = "Phone Number (WhatsApp Preferred)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Current residential address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    [Display(Name = "Current Residential Address")]
    public string CurrentResidentialAddress { get; set; } = string.Empty;

    // Professional Background
    [Display(Name = "Years of Experience in Real Estate/Marketing")]
    [Range(0, 100, ErrorMessage = "Years of experience must be between 0 and 100")]
    public int? YearsOfExperience { get; set; }

    [StringLength(500, ErrorMessage = "Previous firms/agencies cannot exceed 500 characters")]
    [Display(Name = "Previous Firms or Agencies Worked With")]
    public string? PreviousFirmsOrAgencies { get; set; }

    [StringLength(200, ErrorMessage = "Languages spoken cannot exceed 200 characters")]
    [Display(Name = "Languages Spoken")]
    public string? LanguagesSpoken { get; set; }

    // Areas of Specialization
    [Display(Name = "Residential Sales")]
    public bool SpecResidentialSales { get; set; }

    [Display(Name = "Commercial Properties")]
    public bool SpecCommercialProperties { get; set; }

    [Display(Name = "Off-Plan Project Marketing")]
    public bool SpecOffPlanProjectMarketing { get; set; }

    [Display(Name = "Lead Generation")]
    public bool SpecLeadGeneration { get; set; }

    [Display(Name = "Social Media Advertising")]
    public bool SpecSocialMediaAdvertising { get; set; }

    [Display(Name = "Others")]
    public bool SpecOthers { get; set; }

    [StringLength(300, ErrorMessage = "Other specialization description cannot exceed 300 characters")]
    [Display(Name = "Please Specify")]
    public string? SpecOthersDescription { get; set; }

    // Skills and Competencies
    [Display(Name = "Client Prospecting and Networking")]
    public bool SkillClientProspecting { get; set; }

    [Display(Name = "Property Viewing and Tours")]
    public bool SkillPropertyViewing { get; set; }

    [Display(Name = "Local Market Knowledge")]
    public bool SkillMarketKnowledge { get; set; }

    // Availability and Work Structure
    [Display(Name = "Availability")]
    public FreelancerAvailability Availability { get; set; }

    [Display(Name = "Are you willing to work on a commission-only basis?")]
    public bool WillingToWorkOnCommission { get; set; }

    // Portfolio
    [StringLength(1000, ErrorMessage = "Social media links cannot exceed 1000 characters")]
    [Display(Name = "Social Media Links / Personal Website")]
    public string? SocialMediaLinks { get; set; }

    [StringLength(2000, ErrorMessage = "Sales/campaign results cannot exceed 2000 characters")]
    [Display(Name = "Brief Description of Recent Sales/Campaign Results")]
    public string? RecentSalesOrCampaignResults { get; set; }

    // Declaration
    [Display(Name = "I accept the terms and declaration")]
    [MustBeTrue(ErrorMessage = "You must accept the terms and declaration to submit your application")]
    public bool AcceptedTermsAndDeclaration { get; set; }

    [Required(ErrorMessage = "Applicant signature is required")]
    [StringLength(200, ErrorMessage = "Signature cannot exceed 200 characters")]
    [Display(Name = "Applicant's Signature (Type Your Full Name)")]
    public string ApplicantSignature { get; set; } = string.Empty;

    public class Handler : IRequestHandler<SubmitFreelancerApplicationCommand, FreelancerApplication>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FreelancerApplication> Handle(SubmitFreelancerApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = new FreelancerApplication
            {
                // Personal Information
                FullName = request.FullName,
                Nationality = request.Nationality,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                CurrentResidentialAddress = request.CurrentResidentialAddress,

                // Professional Background
                YearsOfExperience = request.YearsOfExperience,
                PreviousFirmsOrAgencies = request.PreviousFirmsOrAgencies,
                LanguagesSpoken = request.LanguagesSpoken,

                // Areas of Specialization
                SpecResidentialSales = request.SpecResidentialSales,
                SpecCommercialProperties = request.SpecCommercialProperties,
                SpecOffPlanProjectMarketing = request.SpecOffPlanProjectMarketing,
                SpecLeadGeneration = request.SpecLeadGeneration,
                SpecSocialMediaAdvertising = request.SpecSocialMediaAdvertising,
                SpecOthers = request.SpecOthers,
                SpecOthersDescription = request.SpecOthersDescription,

                // Skills and Competencies
                SkillClientProspecting = request.SkillClientProspecting,
                SkillPropertyViewing = request.SkillPropertyViewing,
                SkillMarketKnowledge = request.SkillMarketKnowledge,

                // Availability and Work Structure
                Availability = request.Availability,
                WillingToWorkOnCommission = request.WillingToWorkOnCommission,

                // Portfolio
                SocialMediaLinks = request.SocialMediaLinks,
                RecentSalesOrCampaignResults = request.RecentSalesOrCampaignResults,

                // Declaration
                AcceptedTermsAndDeclaration = request.AcceptedTermsAndDeclaration,
                ApplicantSignature = request.ApplicantSignature,
                SignatureDate = DateTime.UtcNow,

                // Metadata
                CreatedAt = DateTime.UtcNow,
                Status = FreelancerApplicationStatus.Pending
            };

            _context.FreelancerApplications.Add(application);
            await _context.SaveChangesAsync(cancellationToken);

            return application;
        }
    }
}
