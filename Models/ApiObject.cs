using System.Text.Json.Serialization;

namespace RestfulApiDevTests.Models;

/// <summary>
/// Represents an API object from restful-api.dev
/// </summary>
public class ApiObject
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, object?>? Data { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents an update request for PATCH operations
/// </summary>
public class ApiObjectUpdate
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, object?>? Data { get; set; }
}

/// <summary>
/// Represents an API error response
/// </summary>
public class ApiError
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}