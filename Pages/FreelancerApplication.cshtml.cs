using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Application.Commands.Freelancers;

namespace SteadyGrowth.Web.Pages;

public class FreelancerApplicationModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<FreelancerApplicationModel> _logger;

    public FreelancerApplicationModel(IMediator mediator, ILogger<FreelancerApplicationModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public SubmitFreelancerApplicationCommand Application { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await _mediator.Send(Application);
            _logger.LogInformation("Freelancer application submitted successfully. ID: {ApplicationId}, Email: {Email}",
                result.Id, result.Email);
            return RedirectToPage("/FreelancerApplicationSuccess");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting freelancer application for {Email}", Application.Email);
            ModelState.AddModelError(string.Empty, "An error occurred while submitting your application. Please try again.");
            return Page();
        }
    }
}
