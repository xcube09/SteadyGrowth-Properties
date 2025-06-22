using SteadyGrowth.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true) => Task.FromResult(false);
        public Task<bool> SendSMSAsync(string phoneNumber, string message) => Task.FromResult(false);
        public Task NotifyPropertyStatusChangeAsync(string userId, int propertyId, PropertyStatus newStatus) => Task.CompletedTask;
        public Task NotifyReferralCommissionAsync(string userId, decimal commission) => Task.CompletedTask;
        public Task NotifyUserRegistrationAsync(string userId, string referrerCode) => Task.CompletedTask;
    }
}
