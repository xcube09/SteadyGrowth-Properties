using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteadyGrowth.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Pages;

/// <summary>
/// Contact page model for handling contact form submissions.
/// </summary>
public class ContactModel : PageModel
{
    private readonly INotificationService _notificationService;

    public ContactModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [BindProperty]
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required, StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [BindProperty]
    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    public string? ResultMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        try
        {
            var body = $"From: {Name} ({Email})\n\n{Message}";
            var sent = await _notificationService.SendEmailAsync("support@steadygrowth.com", Subject, body, false);
            ResultMessage = sent ? "Your message has been sent successfully." : "Failed to send your message. Please try again later.";
        }
        catch
        {
            ResultMessage = "An error occurred while sending your message.";
        }
        return Page();
    }
}
