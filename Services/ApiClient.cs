using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RestfulApiDevTests.Models;

namespace RestfulApiDevTests.Services;

/// <summary>
/// Improved API client with comprehensive error handling and retry logic
/// </summary>
public class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly int _maxRetries;
    private readonly int _retryDelayMs;
    private readonly ILogger? _logger;

    public ApiClient(
        HttpClient httpClient,
        int maxRetries = 3,
        int retryDelayMs = 1000,
        ILogger? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _maxRetries = maxRetries;
        _retryDelayMs = retryDelayMs;
        _logger = logger;
    }

    /// <summary>
    /// PATCH request with comprehensive error handling
    /// </summary>
    public async Task<ApiResponse<ApiObject>> PatchObjectAsync(string id, object updateData)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Object ID cannot be null or empty", nameof(id));
        }

        if (updateData == null)
        {
            throw new ArgumentNullException(nameof(updateData));
        }

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                _logger?.LogInfo($"PATCH /objects/{id}");
                _logger?.LogDebug($"Payload: {JsonSerializer.Serialize(updateData)}");

                var startTime = DateTime.UtcNow;
                var response = await _httpClient.PatchAsync(
                    $"/objects/{id}",
                    JsonContent.Create(updateData)
                );
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                _logger?.LogInfo($"Response: {(int)response.StatusCode} {response.StatusCode} ({duration}ms)");

                var data = response.IsSuccessStatusCode
                    ? await SafeDeserializeAsync<ApiObject>(response)
                    : null;

                return new ApiResponse<ApiObject>(
                    response.StatusCode,
                    data,
                    response.Content.Headers.ContentType?.MediaType,
                    response.IsSuccessStatusCode,
                    (long)duration
                );
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError($"HTTP request failed: {ex.Message}", ex);
                throw new ApiException("HTTP request failed", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger?.LogError($"Request timeout: {ex.Message}", ex);
                throw new ApiException("Request timed out", ex);
            }
            catch (JsonException ex)
            {
                _logger?.LogError($"JSON deserialization failed: {ex.Message}", ex);
                throw new ApiException("Failed to parse API response", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error: {ex.Message}", ex);
                throw new ApiException("Unexpected error occurred", ex);
            }
        });
    }

    /// <summary>
    /// PATCH request with raw JSON string
    /// </summary>
    public async Task<ApiResponse<ApiObject>> PatchObjectRawAsync(string id, string rawJson)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Object ID cannot be null or empty", nameof(id));
        }

        if (rawJson == null)
        {
            throw new ArgumentNullException(nameof(rawJson));
        }

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                _logger?.LogInfo($"PATCH /objects/{id} (raw)");
                _logger?.LogDebug($"Raw JSON: {rawJson}");

                var content = new StringContent(
                    rawJson,
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var startTime = DateTime.UtcNow;
                var response = await _httpClient.PatchAsync($"/objects/{id}", content);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                _logger?.LogInfo($"Response: {(int)response.StatusCode} {response.StatusCode} ({duration}ms)");

                var data = response.IsSuccessStatusCode
                    ? await SafeDeserializeAsync<ApiObject>(response)
                    : null;

                return new ApiResponse<ApiObject>(
                    response.StatusCode,
                    data,
                    response.Content.Headers.ContentType?.MediaType,
                    response.IsSuccessStatusCode,
                    (long)duration
                );
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError($"HTTP request failed: {ex.Message}", ex);
                throw new ApiException("HTTP request failed", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error: {ex.Message}", ex);
                throw new ApiException("Unexpected error occurred", ex);
            }
        });
    }

    /// <summary>
    /// GET request with error handling
    /// </summary>
    public async Task<ApiResponse<ApiObject>> GetObjectAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Object ID cannot be null or empty", nameof(id));
        }

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                _logger?.LogInfo($"GET /objects/{id}");

                var startTime = DateTime.UtcNow;
                var response = await _httpClient.GetAsync($"/objects/{id}");
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                _logger?.LogInfo($"Response: {(int)response.StatusCode} {response.StatusCode} ({duration}ms)");

                var data = response.IsSuccessStatusCode
                    ? await SafeDeserializeAsync<ApiObject>(response)
                    : null;

                return new ApiResponse<ApiObject>(
                    response.StatusCode,
                    data,
                    response.Content.Headers.ContentType?.MediaType,
                    response.IsSuccessStatusCode,
                    (long)duration
                );
            }
            catch (Exception ex)
            {
                _logger?.LogError($"GET request failed: {ex.Message}", ex);
                throw new ApiException("Failed to retrieve object", ex);
            }
        });
    }

    /// <summary>
    /// Safe JSON deserialization with error handling
    /// </summary>
    private async Task<T?> SafeDeserializeAsync<T>(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger?.LogWarning("Response content is empty");
                return default;
            }

            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (JsonException ex)
        {
            _logger?.LogError($"JSON deserialization failed: {ex.Message}", ex);
            return default;
        }
    }

    /// <summary>
    /// Execute operation with retry logic
    /// </summary>
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt < _maxRetries)
        {
            try
            {
                return await operation();
            }
            catch (ApiException ex) when (IsRetryableError(ex) && attempt < _maxRetries - 1)
            {
                attempt++;
                lastException = ex;

                _logger?.LogWarning($"Retry attempt {attempt}/{_maxRetries} after error: {ex.Message}");

                await Task.Delay(_retryDelayMs * attempt); // Exponential backoff
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Non-retryable error: {ex.Message}", ex);
                throw;
            }
        }

        throw new ApiException($"Operation failed after {_maxRetries} attempts", lastException!);
    }

    /// <summary>
    /// Determine if an error is retryable
    /// </summary>
    private bool IsRetryableError(Exception ex)
    {
        return ex.InnerException is HttpRequestException ||
               ex.InnerException is TaskCanceledException ||
               ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// API response with extended information
/// </summary>
public class ApiResponse<T>
{
    public HttpStatusCode StatusCode { get; }
    public T? Data { get; }
    public string? ContentType { get; }
    public bool IsSuccessful { get; }
    public long DurationMs { get; }

    public ApiResponse(
        HttpStatusCode statusCode,
        T? data,
        string? contentType,
        bool isSuccessful,
        long durationMs = 0)
    {
        StatusCode = statusCode;
        Data = data;
        ContentType = contentType;
        IsSuccessful = isSuccessful;
        DurationMs = durationMs;
    }
}

/// <summary>
/// Custom API exception
/// </summary>
public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Simple logger interface
/// </summary>
public interface ILogger
{
    void LogInfo(string message);
    void LogError(string message, Exception? exception = null);
    void LogWarning(string message);
    void LogDebug(string message);
}