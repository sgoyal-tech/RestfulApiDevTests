using RestSharp;
using System.Net;
using RestfulApiDevTests.Models;
using System.Diagnostics;

namespace RestfulApiDevTests.Utilities;

/// <summary>
/// Helper methods for API testing
/// </summary>
public static class ApiHelpers
{
    /// <summary>
    /// Checks if a status code indicates success
    /// </summary>
    public static bool IsSuccessStatusCode(HttpStatusCode statusCode)
    {
        return (int)statusCode >= 200 && (int)statusCode < 300;
    }

    /// <summary>
    /// Extracts error message from response
    /// </summary>
    public static string GetErrorMessage(RestResponse response)
    {
        if (string.IsNullOrEmpty(response.Content))
            return $"HTTP {(int)response.StatusCode}: {response.StatusDescription}";

        try
        {
            var error = System.Text.Json.JsonSerializer.Deserialize<ApiError>(response.Content);
            return error?.Message ?? error?.Error ?? response.Content;
        }
        catch
        {
            return response.Content;
        }
    }

    /// <summary>
    /// Validates that response contains required fields
    /// </summary>
    public static bool ValidateObjectStructure(ApiObject? obj)
    {
        if (obj == null) return false;

        return !string.IsNullOrEmpty(obj.Id) &&
               obj.Name != null;
    }

    /// <summary>
    /// Compares two data dictionaries for equality
    /// </summary>
    public static bool AreDataDictionariesEqual(
        Dictionary<string, object?>? dict1,
        Dictionary<string, object?>? dict2)
    {
        if (dict1 == null && dict2 == null) return true;
        if (dict1 == null || dict2 == null) return false;
        if (dict1.Count != dict2.Count) return false;

        foreach (var key in dict1.Keys)
        {
            if (!dict2.ContainsKey(key)) return false;

            var val1 = dict1[key]?.ToString();
            var val2 = dict2[key]?.ToString();

            if (val1 != val2) return false;
        }

        return true;
    }

    /// <summary>
    /// Waits for API to process (small delay)
    /// </summary>
    public static async Task WaitForApiProcessing(int milliseconds = 500)
    {
        await Task.Delay(milliseconds);
    }

    /// <summary>
    /// Retries an operation with exponential backoff
    /// </summary>
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int initialDelayMs = 1000)
    {
        int retryCount = 0;
        int delay = initialDelayMs;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                    throw;

                await Task.Delay(delay);
                delay *= 2; // Exponential backoff
            }
        }
    }

    /// <summary>
    /// Checks if response time is acceptable
    /// </summary>
    public static bool IsResponseTimeAcceptable(
        TimeSpan responseTime,
        int maxMilliseconds = 2000)
    {
        return responseTime.TotalMilliseconds <= maxMilliseconds;
    }

    /// <summary>
    /// Formats a timestamp for logging
    /// </summary>
    public static string FormatTimestamp(DateTime? timestamp)
    {
        return timestamp?.ToString("yyyy-MM-dd HH:mm:ss.fff UTC") ?? "N/A";
    }

    /// <summary>
    /// Sanitizes object ID for safe usage
    /// </summary>
    public static string SanitizeObjectId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "unknown";

        return id.Trim();
    }

    /// <summary>
    /// Validates HTTP status code is in expected range
    /// </summary>
    public static bool IsStatusCodeInRange(
        HttpStatusCode actual,
        params HttpStatusCode[] expected)
    {
        return expected.Contains(actual);
    }

    /// <summary>
    /// Creates a summary of API response for logging
    /// </summary>
    public static string CreateResponseSummary(RestResponse response)
    {
        return $"Status: {(int)response.StatusCode} {response.StatusCode}, " +
               $"Time: {System.Diagnostics.Stopwatch.GetTimestamp()}ms, " +
               $"Size: {response.Content?.Length ?? 0} bytes";
    }

    /// <summary>
    /// Checks if content type is JSON
    /// </summary>
    public static bool IsJsonContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Generates a test run identifier
    /// </summary>
    public static string GenerateTestRunId()
    {
        return $"TestRun_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Validates that updated timestamp is after created timestamp
    /// </summary>
    public static bool IsUpdateTimestampValid(
        DateTime? createdAt,
        DateTime? updatedAt)
    {
        if (!createdAt.HasValue || !updatedAt.HasValue)
            return false;

        return updatedAt >= createdAt;
    }
}