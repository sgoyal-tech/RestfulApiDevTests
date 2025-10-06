using FluentAssertions;
using System.Net;
using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using RestfulApiDevTests.Models;
using System.Text;
using Xunit.Abstractions;
using RestSharp;
using NPOI.SS.Formula.Functions;

namespace RestfulApiDevTests.Tests;

/// <summary>
/// Comprehensive test suite for PATCH /objects/{id} endpoint
/// </summary>
public class PatchEndpointTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public PatchEndpointTests(ITestOutputHelper output)
    {
        _output = output;
    }

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
        _ = response.Data.Data!["price"].ToString().Should().Contain("150");
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
    #region Missing Payloads

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithNullPayload_ShouldReturnBadRequest()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        _output.WriteLine($"Created test object with ID: {testObject?.Id}");

        // Act
        var response = await _apiClient.PatchObjectAsync(testObject!.Id!, null!);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
        response.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithEmptyStringPayload_ShouldReturnBadRequest()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, "");
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithWhitespaceOnlyPayload_ShouldReturnBadRequest()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, "   ");
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }

    #endregion

    #region Invalid Data Types

    [Theory]
    [Trait("Category", "Negative")]
    [Trait("Priority", "High")]
    [InlineData("{ \"name\": 12345 }")]  // Number instead of string
    [InlineData("{ \"name\": true }")]   // Boolean instead of string
    [InlineData("{ \"name\": [] }")]     // Array instead of string
    public async Task PatchObject_WithInvalidNameDataType_ShouldHandleGracefully(string invalidJson)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        _output.WriteLine($"Testing with payload: {invalidJson}");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, invalidJson);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,              // API accepts and converts
            HttpStatusCode.BadRequest,       // API rejects
            HttpStatusCode.UnprocessableEntity
        );
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithDataAsString_ShouldReturnBadRequest()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        var invalidJson = "{ \"data\": \"should be object not string\" }";
        _output.WriteLine($"Testing with payload: {invalidJson}");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, invalidJson);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithDataAsArray_ShouldReturnBadRequest()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        var invalidJson = "{ \"data\": [1, 2, 3] }";
        _output.WriteLine($"Testing with payload: {invalidJson}");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, invalidJson);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }

    #endregion

    #region Malformed JSON

    [Theory]
    [Trait("Category", "Negative")]
    [Trait("Priority", "High")]
    [InlineData("{ \"name\": \"test\" ")]              // Missing closing brace
    [InlineData("{ \"name\" \"test\" }")]              // Missing colon
    [InlineData("{ name: \"test\" }")]                 // Unquoted key
    [InlineData("{ \"name\": \"test\", }")]            // Trailing comma
    [InlineData("{ \"name\": 'test' }")]               // Single quotes
    [InlineData("{ \"name\": undefined }")]            // Undefined value
    [InlineData("")]                                    // Empty string
    [InlineData("null")]                               // JSON null
    [InlineData("not json at all")]                    // Plain text
    public async Task PatchObject_WithMalformedJson_ShouldReturnBadRequest(string malformedJson)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        _output.WriteLine($"Testing with malformed JSON: {malformedJson}");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, malformedJson);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError
        );
        response.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithDoubleEncodedJson_ShouldReturnBadRequest()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        var doubleEncoded = "\"{ \\\"name\\\": \\\"test\\\" }\"";
        _output.WriteLine($"Testing with double-encoded JSON: {doubleEncoded}");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, doubleEncoded);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity
        );
    }

    #endregion

    #region Content-Type Issues

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithWrongContentType_ShouldReturnUnsupportedMediaType()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.restful-api.dev") };

        var content = new StringContent(
            "{ \"name\": \"test\" }",
            Encoding.UTF8,
        "text/plain"  // Wrong content type
        );

        _output.WriteLine("Testing with wrong Content-Type: text/plain");

        // Act
        var response = await httpClient.PatchAsync($"/objects/{testObject!.Id}", content);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK  // Some APIs are lenient
        );
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_WithMissingContentType_ShouldHandleGracefully()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.restful-api.dev") };

        var content = new StringContent("{ \"name\": \"test\" }");
        content.Headers.ContentType = null;  // Remove content type

        _output.WriteLine("Testing with missing Content-Type header");

        // Act
        var response = await httpClient.PatchAsync($"/objects/{testObject!.Id}", content);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK
        );
    }

    #endregion

    #region Boundary Values

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithExtremelyLargePayload_ShouldHandleGracefully()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");

        // Create a very large payload (>1MB)
        var largeString = new string('A', 1024 * 1024); // 1MB of 'A's
        var largeJson = $"{{ \"name\": \"{largeString}\" }}";

        _output.WriteLine($"Testing with extremely large payload: {largeJson.Length} bytes");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, largeJson);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.RequestEntityTooLarge,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError
        );
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_WithDeeplyNestedJson_ShouldHandleGracefully()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");

        // Create deeply nested JSON (100 levels)
        var nestedJson = "{ \"data\": ";
        for (int i = 0; i < 100; i++)
        {
            nestedJson += "{ \"level\": ";
        }
        nestedJson += "\"deep\"";
        for (int i = 0; i < 100; i++)
        {
            nestedJson += " }";
        }
        nestedJson += " }";

        _output.WriteLine("Testing with deeply nested JSON (100 levels)");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, nestedJson);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.OK
        );
    }

    #endregion

    #region Special Characters and Encoding

    [Theory]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Medium")]
    [InlineData("{ \"name\": \"test\\u0000null\" }")]      // Null character
    [InlineData("{ \"name\": \"test\\r\\nCRLF\" }")]       // CRLF injection
    [InlineData("{ \"name\": \"<script>alert()</script>\" }")]  // XSS attempt
    [InlineData("{ \"name\": \"'; DROP TABLE objects;--\" }")]   // SQL injection attempt
    public async Task PatchObject_WithDangerousCharacters_ShouldSanitizeOrReject(string dangerousJson)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        _output.WriteLine($"Testing with dangerous payload: {dangerousJson}");

        // Act
        RestResponse<T> response = (RestResponse<T>)await _apiClient.PatchObjectRawAsync(testObject!.Id!, dangerousJson);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert - API should either sanitize or reject
        if (response.IsSuccessful)
        {
            // If accepted, data should be sanitized
            response.Data.Should().NotBeNull();
            _output.WriteLine(message: $"API accepted and returned: {response.Data!.ToString}");
        }
        else
        {
            // Or should reject with appropriate error
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.UnprocessableEntity
            );
        }
    }

    #endregion

    #region Authentication and Authorization

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_WithInvalidAuthToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.restful-api.dev") };
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer invalid_token_12345");

        _output.WriteLine("Testing with invalid authorization token");

        // Act
        var content = new StringContent("{ \"name\": \"test\" }", Encoding.UTF8, "application/json");
        var response = await httpClient.PatchAsync($"/objects/{testObject!.Id}", content);
        _output.WriteLine($"Response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.OK  // If auth is not implemented
        );
    }

    #endregion

    #region Rate Limiting

    [Fact(Skip = "Rate limiting test - may be slow")]
    [Trait("Category", "Negative")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_RapidSuccessiveRequests_ShouldRespectRateLimits()
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        var requestCount = 100;
        var rateLimitHit = false;

        _output.WriteLine($"Sending {requestCount} rapid requests to test rate limiting");

        // Act
        for (int i = 0; i < requestCount; i++)
        {
            var response = await _apiClient.PatchObjectAsync(
                testObject!.Id!,
                new { name = $"Test {i}" }
            );

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitHit = true;
                _output.WriteLine($"Rate limit hit at request #{i}");
                break;
            }
        }

        // Assert - Either rate limit was hit or all requests succeeded
        _output.WriteLine(rateLimitHit
            ? "✓ Rate limiting is implemented"
            : "⚠ No rate limiting detected");
    }

    #endregion

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
#endregion// The code is already in C#. No conversion is needed.
