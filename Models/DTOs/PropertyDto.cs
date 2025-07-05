using System.Text.Json;
using System.Text.Json.Serialization;
using SteadyGrowth.Web.Models.Entities;

namespace SteadyGrowth.Web.Models.DTOs;

/// <summary>
/// Data Transfer Object for Property.
/// </summary>
public class PropertyDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("propertyType")]
    public PropertyType PropertyType { get; set; }

    [JsonPropertyName("status")]
    public PropertyStatus Status { get; set; }


    [JsonPropertyName("features")]
    public List<string> Features { get; set; } = new();

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("approvedAt")]
    public DateTime? ApprovedAt { get; set; }

    [JsonPropertyName("owner")]
    public UserDto? Owner { get; set; }

    [JsonPropertyName("propertyImages")]
    public List<PropertyImageDto> PropertyImages { get; set; } = new();

    /// <summary>
    /// Implicit conversion from Property entity to PropertyDto.
    /// </summary>
    /// <param name="entity">The Property entity.</param>
    public static implicit operator PropertyDto(Property entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        Price = entity.Price,
        Location = entity.Location,
        PropertyType = entity.PropertyType,
        Status = entity.Status,
        Features = string.IsNullOrWhiteSpace(entity.Features) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(entity.Features) ?? new List<string>(),
        CreatedAt = entity.CreatedAt,
        ApprovedAt = entity.ApprovedAt,
        Owner = entity.User is not null ? (UserDto)entity.User : null,
        PropertyImages = entity.PropertyImages?.OrderBy(pi => pi.DisplayOrder).Select(pi => new PropertyImageDto
        {
            Id = pi.Id,
            FileName = pi.FileName,
            Caption = pi.Caption,
            ImageType = pi.ImageType,
            DisplayOrder = pi.DisplayOrder
        }).ToList() ?? new List<PropertyImageDto>()
    };
}
