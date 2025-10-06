using RestfulApiDevTests.Services;
using RestfulApiDevTests.Models;
using RestfulApiDevTests.Utilities;
using Xunit.Abstractions;
using System.Net.Http.Json;

namespace RestfulApiDevTests.Tests;

/// <summary>
/// Enhanced base class with integrated logging support
/// Use this for tests that need detailed logging
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly ApiClient _apiClient;
    protected readonly HttpClient _httpClient;
    protected readonly TestLogger _logger;
    protected readonly List<string> _createdObjectIds;
    private readonly ITestOutputHelper _output;

    protected TestBase(ITestOutputHelper output)
    {
        _output = output;
        _logger = new TestLogger(output);

        // Initialize HttpClient with base address
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.restful-api.dev"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Create a simple logger adapter for ImprovedApiClient
        var apiLogger = new SimpleLogger(_logger);

        // Initialize ImprovedApiClient with retry logic and logging
        _apiClient = new ApiClient(
            _httpClient,
            maxRetries: 3,
            retryDelayMs: 1000,
            logger: apiLogger
        );

        _createdObjectIds = new List<string>();

        _logger.LogInfo("Test base initialized");
    }

    /// <summary>
    /// Creates a test object with logging
    /// </summary>
    protected async Task<ApiObject?> CreateTestObjectAsync(string name, Dictionary<string, object?>? data = null)
    {
        _logger.LogInfo($"Creating test object: {name}");

        try
        {
            var objectData = new
            {
                name = name,
                data = data ?? new Dictionary<string, object?>
                {
                    { "test", true },
                    { "createdBy", "AutomatedTest" },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                }
            };

            _logger.LogApiRequest("POST", "/objects", objectData);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClient.PostAsJsonAsync("/objects", objectData);
            stopwatch.Stop();

            _logger.LogApiResponse(
                (int)response.StatusCode,
                response.StatusCode.ToString(),
                null,
                stopwatch.ElapsedMilliseconds
            );

            if (response.IsSuccessStatusCode)
            {
                var createdObject = await response.Content.ReadFromJsonAsync<ApiObject>();

                if (createdObject?.Id != null)
                {
                    _createdObjectIds.Add(createdObject.Id);
                    _logger.LogInfo($"✓ Created object with ID: {createdObject.Id}");
                    return createdObject;
                }
            }

            _logger.LogError($"Failed to create object. Status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception creating test object: {ex.Message}", ex);
            return null;
        }
    }

    /// <summary>
    /// Creates multiple test objects with progress logging
    /// </summary>
    protected async Task<List<ApiObject>> CreateMultipleTestObjectsAsync(int count, string namePrefix = "Test Object")
    {
        _logger.LogInfo($"Creating {count} test objects with prefix '{namePrefix}'");
        var objects = new List<ApiObject>();

        for (int i = 1; i <= count; i++)
        {
            _logger.LogDebug($"Creating object {i}/{count}");
            var obj = await CreateTestObjectAsync($"{namePrefix} {i}");
            if (obj != null)
            {
                objects.Add(obj);
            }
            else
            {
                _logger.LogWarning($"Failed to create object {i}/{count}");
            }
        }

        _logger.LogInfo($"✓ Successfully created {objects.Count}/{count} objects");
        return objects;
    }

    /// <summary>
    /// Cleanup with detailed logging
    /// </summary>
    protected async Task CleanupCreatedObjectsAsync()
    {
        if (_createdObjectIds.Count == 0)
        {
            _logger.LogDebug("No objects to clean up");
            return;
        }

        _logger.LogInfo($"Cleaning up {_createdObjectIds.Count} created objects");
        int successCount = 0;
        int failCount = 0;

        foreach (var id in _createdObjectIds.ToList())
        {
            try
            {
                _logger.LogDebug($"Deleting object: {id}");
                var response = await _httpClient.DeleteAsync($"/objects/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _createdObjectIds.Remove(id);
                    successCount++;
                    _logger.LogDebug($"✓ Deleted object: {id}");
                }
                else
                {
                    failCount++;
                    _logger.LogWarning($"Failed to delete object {id}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                failCount++;
                _logger.LogError($"Exception deleting object {id}: {ex.Message}");
            }
        }

        _logger.LogInfo($"Cleanup complete: {successCount} deleted, {failCount} failed");
    }

    /// <summary>
    /// Verify object exists with logging
    /// </summary>
    protected async Task<bool> VerifyObjectExistsAsync(string objectId)
    {
        _logger.LogDebug($"Verifying object exists: {objectId}");

        try
        {
            var response = await _apiClient.GetObjectAsync(objectId);
            var exists = response.IsSuccessful && response.Data != null;

            if (exists)
            {
                _logger.LogDebug($"✓ Object {objectId} exists");
            }
            else
            {
                _logger.LogDebug($"✗ Object {objectId} does not exist");
            }

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying object {objectId}: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// Wait with logging
    /// </summary>
    protected async Task WaitAsync(int milliseconds, string reason = "")
    {
        var message = string.IsNullOrEmpty(reason)
            ? $"Waiting {milliseconds}ms"
            : $"Waiting {milliseconds}ms: {reason}";

        _logger.LogDebug(message);
        await Task.Delay(milliseconds);
    }

    /// <summary>
    /// Get random mock object ID
    /// </summary>
    protected string GetRandomMockObjectId()
    {
        var random = new Random();
        var mockIds = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13" };
        var selectedId = mockIds[random.Next(mockIds.Length)];
        _logger.LogDebug($"Selected random mock object ID: {selectedId}");
        return selectedId;
    }

    /// <summary>
    /// Start a test with logging
    /// </summary>
    protected void StartTest(string testName)
    {
        _logger.LogTestStart(testName);
    }

    /// <summary>
    /// End a test with logging
    /// </summary>
    protected void EndTest(string testName, bool passed)
    {
        _logger.LogTestEnd(testName, passed);
        _output.WriteLine(_logger.GetLogSummary());
    }

    /// <summary>
    /// Dispose pattern implementation
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logger.LogInfo("Disposing test base");

            try
            {
                CleanupCreatedObjectsAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cleanup error: {ex.Message}", ex);
            }

            _apiClient?.Dispose();
            _httpClient?.Dispose();

            _logger.LogInfo("Test base disposed");
        }
    }
}

/// <summary>
/// Simple logger adapter for ImprovedApiClient
/// </summary>
public class SimpleLogger : ILogger
{
    private readonly TestLogger _testLogger;

    public SimpleLogger(TestLogger testLogger)
    {
        _testLogger = testLogger;
    }

    public void LogInfo(string message) => _testLogger.LogInfo(message);
    public void LogError(string message, Exception? exception = null) => _testLogger.LogError(message, exception);
    public void LogWarning(string message) => _testLogger.LogWarning(message);
    public void LogDebug(string message) => _testLogger.LogDebug(message);
}