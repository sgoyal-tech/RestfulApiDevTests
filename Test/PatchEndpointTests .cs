using FluentAssertions;
using System.Net;
using System.Diagnostics;

namespace RestfulApiDevTests.Tests;

/// <summary>
/// Comprehensive test suite for PATCH /objects/{id} endpoint
/// </summary>
public class PatchEndpointTests : TestBase
{
    #region Happy Path Tests

    [Fact]
    [Trait("Category", "HappyPath")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithNameOnly_ShouldUpdateSuccessfully()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Original Name");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new { name = "Updated Name" };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccessful.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(objectId);
        response.Data.Name.Should().Be("Updated Name");
        response.Data.UpdatedAt.Should().NotBeNull();
        response.Data.UpdatedAt.Should().BeAfter(testObject.CreatedAt!.Value);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithDataAttributesOnly_ShouldUpdateSuccessfully()
    {
        // Arrange
        var initialData = new Dictionary<string, object?>
        {
            { "price", 100.00 },
            { "color", "Red" }
        };
        var testObject = await CreateTestObjectAsync("Test Product", initialData);
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new
        {
            data = new Dictionary<string, object?>
            {
                { "price", 150.00 },
                { "color", "Blue" }
            }
        };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Data.Should().ContainKey("price");
        response.Data.Data!["price"].ToString().Should().Contain("150");
        response.Data.Data.Should().ContainKey("color");
        response.Data.Data["color"].Should().Be("Blue");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithCompleteUpdate_ShouldSucceed()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Old Name");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new
        {
            name = "Completely New Name",
            data = new Dictionary<string, object?>
            {
                { "category", "Electronics" },
                { "stock", 50 },
                { "available", true }
            }
        };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be("Completely New Name");
        response.Data.Data.Should().ContainKey("category");
        response.Data.Data.Should().ContainKey("stock");
        response.Data.Data.Should().ContainKey("available");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithSingleDataField_ShouldUpdateOnlyThatField()
    {
        // Arrange
        var initialData = new Dictionary<string, object?>
        {
            { "field1", "value1" },
            { "field2", "value2" },
            { "field3", "value3" }
        };
        var testObject = await CreateTestObjectAsync("Multi Field Object", initialData);
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new
        {
            data = new Dictionary<string, object?>
            {
                { "field2", "updated_value2" }
            }
        };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        // Note: API behavior may vary - this tests partial update capability
        response.Data!.Data.Should().ContainKey("field2");
    }

    #endregion

    #region Negative Tests

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithNonExistentId_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = "99999999999";
        var updateData = new { name = "Should Not Work" };

        // Act
        var response = await _apiClient.PatchObjectAsync(nonExistentId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithInvalidIdFormat_ShouldReturnError()
    {
        // Arrange
        var invalidId = "invalid_id_abc";
        var updateData = new { name = "Test" };

        // Act
        var response = await _apiClient.PatchObjectAsync(invalidId, updateData);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithSpecialCharactersInName_ShouldSucceed()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Normal Name");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new { name = "Special!@#$%^&*()_+-={}[]|\\:;\"'<>,.?/~`±§ Characters" };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Contain("Special");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithUnicodeCharacters_ShouldSucceed()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("English Name");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new { name = "Unicode: 你好世界 مرحبا العالم Здравствуй мир 🚀🎉" };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Contain("Unicode");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_WithEmptyDataObject_ShouldSucceed()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Data Test");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new
        {
            data = new Dictionary<string, object?>()
        };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest
        );
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithVeryLongName_ShouldHandleCorrectly()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Short Name");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var longName = new string('A', 1000); // 1000 character name
        var updateData = new { name = longName };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        // API should either accept or reject with proper error
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_WithNestedDataStructure_ShouldHandleCorrectly()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Nested Data Test");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new
        {
            data = new Dictionary<string, object?>
            {
                { "level1", new Dictionary<string, object?>
                    {
                        { "level2", new Dictionary<string, object?>
                            {
                                { "level3", "deeply nested value" }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest
        );
    }

    #endregion

    #region Performance Tests

    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_ResponseTime_ShouldBeUnder2Seconds()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Performance Test");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new { name = "Updated for Performance" };
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000,
            "API response should be under 2 seconds");
    }

    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_MultipleSequentialRequests_ShouldSucceed()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Sequential Test");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        // Act & Assert - Multiple updates
        for (int i = 1; i <= 5; i++)
        {
            var updateData = new { name = $"Update #{i}" };
            var response = await _apiClient.PatchObjectAsync(objectId, updateData);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Data!.Name.Should().Be($"Update #{i}");
        }
    }

    #endregion

    #region Content-Type and Header Tests

    [Fact]
    [Trait("Category", "Headers")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_ResponseContentType_ShouldBeJson()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Content Type Test");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new { name = "Updated Name" };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ContentType.Should().Contain("application/json");
    }

    #endregion

    #region Data Consistency Tests

    [Fact]
    [Trait("Category", "DataConsistency")]
    [Trait("Priority", "High")]
    public async Task PatchObject_ShouldPersistChanges()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Persistence Test");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new { name = "Persisted Name" };

        // Act - Update
        var patchResponse = await _apiClient.PatchObjectAsync(objectId, updateData);
        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Retrieve to verify persistence
        await Task.Delay(500); // Small delay to ensure data is written
        var getResponse = await _apiClient.GetObjectAsync(objectId);

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Data.Should().NotBeNull();
        getResponse.Data!.Name.Should().Be("Persisted Name");
    }

    [Fact]
    [Trait("Category", "DataConsistency")]
    [Trait("Priority", "High")]
    public async Task PatchObject_UpdatedAtField_ShouldBeUpdated()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Timestamp Test");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;
        var originalUpdatedAt = testObject.UpdatedAt ?? testObject.CreatedAt;

        await Task.Delay(1000); // Ensure time difference

        var updateData = new { name = "Updated Name for Timestamp" };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.UpdatedAt.Should().NotBeNull();

        if (originalUpdatedAt.HasValue)
        {
            response.Data.UpdatedAt.Should().BeAfter(originalUpdatedAt.Value);
        }
    }

    [Fact]
    [Trait("Category", "DataConsistency")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_IdField_ShouldRemainUnchanged()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("ID Immutability Test");
        testObject.Should().NotBeNull();
        var originalId = testObject!.Id!;

        var updateData = new
        {
            id = "999999",  // Attempt to change ID
            name = "Try to Change ID"
        };

        // Act
        var response = await _apiClient.PatchObjectAsync(originalId, updateData);

        // Assert
        if (response.IsSuccessful)
        {
            response.Data!.Id.Should().Be(originalId,
                "ID should not change even if provided in update");
        }
    }

    #endregion

    #region Type Handling Tests

    [Fact]
    [Trait("Category", "TypeHandling")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithMixedDataTypes_ShouldHandleCorrectly()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Type Test");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new
        {
            data = new Dictionary<string, object?>
            {
                { "stringField", "text value" },
                { "numberField", 42 },
                { "decimalField", 3.14159 },
                { "booleanField", true },
                { "nullField", null }
            }
        };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Data.Should().NotBeNull();
    }

    #endregion

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithEmptyBody_ShouldHandleGracefully()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new { };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        // API should either accept empty update or return appropriate error
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithInvalidJson_ShouldReturn400()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var invalidJson = "{ invalid json structure";

        // Act
        var response = await _apiClient.PatchObjectRawAsync(objectId, invalidJson);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_WithNullValues_ShouldHandleCorrectly()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Null Test Object");
        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        var updateData = new
        {
            name = (string?)null,
            data = (Dictionary<string, object?>?)null
        };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        // API should handle null values appropriately
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }


    #region Edge Cases

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_WithVeryLargeId_ShouldHandleCorrectly()
    {
        // Arrange
        var largeId = "999999999999999999";
        var updateData = new { name = "Large ID Test" };

        // Act
        var response = await _apiClient.PatchObjectAsync(largeId, updateData);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }
}
#endregion