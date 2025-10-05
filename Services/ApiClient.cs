using RestSharp;
using System.Text.Json;
using RestfulApiDevTests.Models;

namespace RestfulApiDevTests.Services;

/// <summary>
/// Client for interacting with the restful-api.dev API
/// </summary>
public class ApiClient : IDisposable
{
    private readonly RestClient _client;
    private readonly string _baseUrl = "https://api.restful-api.dev";

    public ApiClient()
    {
        var options = new RestClientOptions(_baseUrl)
        {
            ThrowOnAnyError = false,
            MaxTimeout = 30000 // 30 seconds
        };
        _client = new RestClient(options);
    }

    /// <summary>
    /// Get an object by ID
    /// </summary>
    public async Task<RestResponse<ApiObject>> GetObjectAsync(string objectId)
    {
        var request = new RestRequest($"/objects/{objectId}", Method.Get);
        return await _client.ExecuteAsync<ApiObject>(request);
    }

    /// <summary>
    /// Create a new object
    /// </summary>
    public async Task<RestResponse<ApiObject>> CreateObjectAsync(ApiObjectUpdate data)
    {
        var request = new RestRequest("/objects", Method.Post);
        request.AddJsonBody(data);
        return await _client.ExecuteAsync<ApiObject>(request);
    }

    /// <summary>
    /// Partially update an object using PATCH
    /// </summary>
    public async Task<RestResponse<ApiObject>> PatchObjectAsync(string objectId, object updateData)
    {
        var request = new RestRequest($"/objects/{objectId}", Method.Patch);
        request.AddJsonBody(updateData);
        return await _client.ExecuteAsync<ApiObject>(request);
    }

    /// <summary>
    /// Fully update an object using PUT
    /// </summary>
    public async Task<RestResponse<ApiObject>> PutObjectAsync(string objectId, ApiObjectUpdate data)
    {
        var request = new RestRequest($"/objects/{objectId}", Method.Put);
        request.AddJsonBody(data);
        return await _client.ExecuteAsync<ApiObject>(request);
    }

    /// <summary>
    /// Delete an object
    /// </summary>
    public async Task<RestResponse> DeleteObjectAsync(string objectId)
    {
        var request = new RestRequest($"/objects/{objectId}", Method.Delete);
        return await _client.ExecuteAsync(request);
    }

    /// <summary>
    /// Send a raw PATCH request with custom body
    /// </summary>
    public async Task<RestResponse> PatchObjectRawAsync(string objectId, string jsonBody)
    {
        var request = new RestRequest($"/objects/{objectId}", Method.Patch);
        request.AddStringBody(jsonBody, ContentType.Json);
        return await _client.ExecuteAsync(request);
    }

    /// <summary>
    /// Get error information from response
    /// </summary>
    public ApiError? GetErrorFromResponse(RestResponse response)
    {
        if (string.IsNullOrEmpty(response.Content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ApiError>(response.Content);
        }
        catch
        {
            return new ApiError { Error = "Unknown", Message = response.Content };
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}