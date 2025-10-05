using RestfulApiDevTests.Services;
using RestfulApiDevTests.Models;

namespace RestfulApiDevTests.Tests;

/// <summary>
/// Base class for all test classes with common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly ApiClient _apiClient;
    protected readonly List<string> _createdObjectIds;

    protected TestBase()
    {
        _apiClient = new ApiClient();
        _createdObjectIds = new List<string>();
    }

    /// <summary>
    /// Creates a test object and tracks it for cleanup
    /// </summary>
    protected async Task<ApiObject?> CreateTestObjectAsync(string name, Dictionary<string, object?>? data = null)
    {
        var objectData = new ApiObjectUpdate
        {
            Name = name,
            Data = data ?? new Dictionary<string, object?>
            {
                { "test", true },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            }
        };

        var response = await _apiClient.CreateObjectAsync(objectData);

        if (response.IsSuccessful && response.Data?.Id != null)
        {
            _createdObjectIds.Add(response.Data.Id);
            return response.Data;
        }

        return null;
    }

    /// <summary>
    /// Cleanup: Delete all created objects
    /// </summary>
    protected async Task CleanupCreatedObjectsAsync()
    {
        foreach (var id in _createdObjectIds)
        {
            try
            {
                await _apiClient.DeleteObjectAsync(id);
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
        _createdObjectIds.Clear();
    }

    public void Dispose()
    {
        // Synchronous cleanup
        Task.Run(async () => await CleanupCreatedObjectsAsync()).Wait();
        _apiClient?.Dispose();
    }
}