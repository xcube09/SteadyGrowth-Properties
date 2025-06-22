using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Interfaces;

/// <summary>
/// Service interface for sending notifications to users.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an email notification.
    /// </summary>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Sends an SMS notification.
    /// </summary>
    Task<bool> SendSMSAsync(string phoneNumber, string message);

    /// <summary>
    /// Notifies a user about a property status change.
    /// </summary>
    Task NotifyPropertyStatusChangeAsync(string userId, int propertyId, PropertyStatus newStatus);

    /// <summary>
    /// Notifies a user about referral commission earned.
    /// </summary>
    Task NotifyReferralCommissionAsync(string userId, decimal commission);

    /// <summary>
    /// Notifies a user and their referrer about registration.
    /// </summary>
    Task NotifyUserRegistrationAsync(string userId, string referrerCode);
}
