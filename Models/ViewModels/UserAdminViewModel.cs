using System;

namespace SteadyGrowth.Web.Models.ViewModels
{
    public class UserAdminViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public decimal WalletBalance { get; set; }
    }

    public class AuditLogViewModel
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
    }
}
