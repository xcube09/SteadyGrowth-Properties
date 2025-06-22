using System.Text.Json.Serialization;

namespace SteadyGrowth.Web.Models.DTOs;

/// <summary>
/// Generic API response DTO.
/// </summary>
public class ApiResponseDto<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponseDto{T}"/> class.
    /// </summary>
    public ApiResponseDto() { }
    public ApiResponseDto(bool success, string message, T? data = default, List<string>? errors = null)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors ?? new List<string>();
    }
}
