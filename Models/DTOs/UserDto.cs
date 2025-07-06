using System.Text.Json.Serialization;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Models.DTOs;

/// <summary>
/// Data Transfer Object for User.
/// </summary>
public class UserDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("referralCode")]
    public string? ReferralCode { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("totalProperties")]
    public int TotalProperties { get; set; }

    [JsonPropertyName("totalReferrals")]
    public int TotalReferrals { get; set; }

    /// <summary>
    /// Implicit conversion from User entity to UserDto.
    /// </summary>
    /// <param name="entity">The User entity.</param>
    public static implicit operator UserDto(User entity) => new()
    {
        Id = entity.Id,
        Email = entity.Email!,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        PhoneNumber = entity.PhoneNumber,
        ReferralCode = entity.ReferralCode,
        CreatedAt = entity.CreatedAt,
        IsActive = entity.IsActive,
        TotalProperties = entity.Properties?.Count ?? 0,
        TotalReferrals = entity.ReferralsMade?.Count ?? 0
    };
}
