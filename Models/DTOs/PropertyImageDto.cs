using System;
using System.Text.Json.Serialization;

namespace SteadyGrowth.Web.Models.DTOs
{
    /// <summary>
    /// DTO for property images.
    /// </summary>
    public class PropertyImageDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("imageType")]
        public string? ImageType { get; set; }

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }
    }
}
