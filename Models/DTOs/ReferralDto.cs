using System.Text.Json.Serialization;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Models.DTOs;

/// <summary>
/// Data Transfer Object for Referral.
/// </summary>
public class ReferralDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("referrerId")]
    public string ReferrerId { get; set; } = string.Empty;

    [JsonPropertyName("referredUserId")]
    public string ReferredUserId { get; set; } = string.Empty;

    [JsonPropertyName("commissionEarned")]
    public decimal CommissionEarned { get; set; }

    [JsonPropertyName("commissionPaid")]
    public bool CommissionPaid { get; set; }

    [JsonPropertyName("paidAt")]
    public DateTime? PaidAt { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("referrer")]
    public UserDto? Referrer { get; set; }

    [JsonPropertyName("referredUser")]
    public UserDto? ReferredUser { get; set; }

    /// <summary>
    /// Implicit conversion from Referral entity to ReferralDto.
    /// </summary>
    /// <param name="entity">The Referral entity.</param>
    public static implicit operator ReferralDto(Referral entity) => new()
    {
        Id = entity.Id,
        ReferrerId = entity.ReferrerId,
        ReferredUserId = entity.ReferredUserId,
        CommissionEarned = entity.CommissionEarned,
        CommissionPaid = entity.CommissionPaid,
        PaidAt = entity.PaidAt,
        CreatedAt = entity.CreatedAt,
        IsActive = entity.IsActive,
        Referrer = entity.Referrer is not null ? (UserDto)entity.Referrer : null,
        ReferredUser = entity.ReferredUser is not null ? (UserDto)entity.ReferredUser : null
    };
}
