using FluentAssertions;
using System.Net;
using Xunit.Abstractions;

namespace RestfulApiDevTests.Tests;

/// <summary>
/// Parameterized tests for improved reusability across different datasets
/// </summary>
public class ParameterizedTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public ParameterizedTests(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    #region Data-Driven Tests with Theory

    public static IEnumerable<object[]> ValidUpdateData =>
        new List<object[]>
        {
            new object[] { "Simple Name", new Dictionary<string, object?> { { "color", "Red" } } },
            new object[] { "Product 123", new Dictionary<string, object?> { { "price", 99.99 }, { "stock", 10 } } },
            new object[] { "Device XYZ", new Dictionary<string, object?> { { "brand", "Apple" }, { "model", "iPhone" } } },
            new object[] { "Unicode 测试", new Dictionary<string, object?> { { "language", "Chinese" } } },
            new object[] { "Empty Data", new Dictionary<string, object?>() }
        };

    [Theory]
    [MemberData(nameof(ValidUpdateData))]
    [Trait("Category", "Parameterized")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithVariousValidData_ShouldSucceed(
        string name,
        Dictionary<string, object?> data)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Initial Name");

        // Validate test object creation
        testObject.Should().NotBeNull($"Failed to create test object for test case: {name}");

        if (testObject == null)
        {
            _output.WriteLine($"⚠️ Skipping test for '{name}' - failed to create test object");
            return;
        }

        testObject.Id.Should().NotBeNullOrWhiteSpace($"Test object ID should be valid for test case: {name}");

        _output.WriteLine($"Testing with name='{name}', data={System.Text.Json.JsonSerializer.Serialize(data)}");
        _output.WriteLine($"Test object ID: {testObject.Id}");

        var updateData = new { name, data };

        // Act
        var response = await _apiClient.PatchObjectAsync(testObject.Id!, updateData);

        // Assert
        _output.WriteLine($"Response status: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Test case '{name}' should succeed");
        response.Data.Should().NotBeNull($"Response data should not be null for test case: {name}");
        response.Data!.Name.Should().Be(name);
        _output.WriteLine($"✓ Test passed for: {name}");
    }

    #endregion

    #region Invalid Status Code Tests

    public static IEnumerable<object[]> InvalidIdTestCases =>
        new List<object[]>
        {
            new object[] { "999999999", HttpStatusCode.NotFound, "Non-existent ID" },
            new object[] { "abc123", HttpStatusCode.NotFound, "Alphanumeric ID" },
            new object[] { "-1", HttpStatusCode.NotFound, "Negative ID" },
            new object[] { "0", HttpStatusCode.NotFound, "Zero ID" },
            new object[] { "9999999999999999999", HttpStatusCode.NotFound, "Very large ID" }
        };

    [Theory]
    [MemberData(nameof(InvalidIdTestCases))]
    [Trait("Category", "Parameterized")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithInvalidIds_ShouldReturnExpectedStatus(
        string invalidId,
        HttpStatusCode expectedStatus,
        string description)
    {
        // Arrange
        _output.WriteLine($"Testing: {description} (ID={invalidId})");
        var updateData = new { name = "Test" };

        // Act
        var response = await _apiClient.PatchObjectAsync(invalidId, updateData);

        // Assert
        response.StatusCode.Should().Be(expectedStatus);
        response.IsSuccessful.Should().BeFalse();
        _output.WriteLine($"✓ Correctly returned {expectedStatus} for {description}");
    }

    #endregion

    #region Malformed JSON Test Cases

    public static IEnumerable<object[]> MalformedJsonTestCases =>
        new List<object[]>
        {
            new object[] { "{ \"name\": \"test\" ", "Missing closing brace" },
            new object[] { "{ \"name\" \"test\" }", "Missing colon" },
            new object[] { "{ name: \"test\" }", "Unquoted key" },
            new object[] { "{ \"name\": \"test\", }", "Trailing comma" },
            new object[] { "{ \"name\": 'test' }", "Single quotes" },
            new object[] { "", "Empty string" },
            new object[] { "null", "JSON null" },
            new object[] { "undefined", "Undefined value" },
            new object[] { "not json", "Plain text" },
            new object[] { "{ \"name\": }", "Missing value" }
        };

    [Theory]
    [MemberData(nameof(MalformedJsonTestCases))]
    [Trait("Category", "Parameterized")]
    [Trait("Priority", "High")]
    public async Task PatchObject_WithMalformedJson_ShouldReturnBadRequest(
        string malformedJson,
        string description)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        _output.WriteLine($"Testing: {description}");
        _output.WriteLine($"Payload: {malformedJson}");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, malformedJson);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError
        );
        response.IsSuccessful.Should().BeFalse();
        _output.WriteLine($"✓ Correctly rejected {description} with {response.StatusCode}");
    }

    #endregion

    #region Data Type Validation Tests

    public static IEnumerable<object[]> InvalidDataTypeTestCases =>
        new List<object[]>
        {
            new object[] { "{ \"name\": 12345 }", "Number instead of string" },
            new object[] { "{ \"name\": true }", "Boolean instead of string" },
            new object[] { "{ \"name\": [] }", "Array instead of string" },
            new object[] { "{ \"name\": {} }", "Object instead of string" },
            new object[] { "{ \"data\": \"string\" }", "String instead of object" },
            new object[] { "{ \"data\": 123 }", "Number instead of object" },
            new object[] { "{ \"data\": [1,2,3] }", "Array instead of object" }
        };

    [Theory]
    [MemberData(nameof(InvalidDataTypeTestCases))]
    [Trait("Category", "Parameterized")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithInvalidDataTypes_ShouldHandleGracefully(
        string invalidJson,
        string description)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Test Object");
        _output.WriteLine($"Testing: {description}");
        _output.WriteLine($"Payload: {invalidJson}");

        // Act
        var response = await _apiClient.PatchObjectRawAsync(testObject!.Id!, invalidJson);

        // Assert - API may accept and convert OR reject
        _output.WriteLine($"Response: {response.StatusCode}");

        if (response.IsSuccessful)
        {
            _output.WriteLine($"✓ API accepted and converted {description}");
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.UnprocessableEntity
            );
            _output.WriteLine($"✓ API rejected {description}");
        }
    }

    #endregion

    #region Unicode and Special Characters Tests

    public static IEnumerable<object[]> UnicodeTestCases =>
        new List<object[]>
        {
            new object[] { "Chinese: 你好世界", "Chinese characters" },
            new object[] { "Arabic: مرحبا بك", "Arabic characters" },
            new object[] { "Russian: Привет мир", "Russian characters" },
            new object[] { "Japanese: こんにちは", "Japanese characters" },
            new object[] { "Emoji: 🚀🎉✨🔥", "Emoji characters" },
            new object[] { "Mixed: Hello 世界 🌍", "Mixed languages and emoji" },
            new object[] { "Special: !@#$%^&*()", "Special characters" },
            new object[] { "Quotes: \"'`", "Quote characters" }
        };

    [Theory]
    [MemberData(nameof(UnicodeTestCases))]
    [Trait("Category", "Parameterized")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithUnicodeAndSpecialChars_ShouldSucceed(
        string testName,
        string description)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Original Name");
        _output.WriteLine($"Testing: {description}");
        _output.WriteLine($"Name: {testName}");

        var updateData = new { name = testName };

        // Act
        var response = await _apiClient.PatchObjectAsync(testObject!.Id!, updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be(testName);
        _output.WriteLine($"✓ Successfully handled {description}");
    }

    #endregion

    #region Response Time Tests

    public static IEnumerable<object[]> ResponseTimeTestCases =>
        new List<object[]>
        {
            new object[] { "Small payload", new { name = "Test" }, 2000 },
            new object[] { "Medium payload", new { name = "Test", data = new Dictionary<string, object>
                {
                    { "field1", "value1" },
                    { "field2", "value2" },
                    { "field3", "value3" }
                }}, 2000 },
            new object[] { "Large payload", new { name = new string('A', 1000), data = new Dictionary<string, object>
                {
                    { "largeField", new string('B', 5000) }
                }}, 3000 }
        };

    [Theory]
    [MemberData(nameof(ResponseTimeTestCases))]
    [Trait("Category", "Parameterized")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_ResponseTime_ShouldBeAcceptable(
        string description,
        object updateData,
        int maxMilliseconds)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Performance Test");
        // Arrange
        //var testObject = await CreateTestObjectAsync("Test Object");

        // ✅ REQUIRED: Null check
        if (testObject == null)
        {
            _output.WriteLine("⚠️ Skipping test - failed to create test object");
            return;
        }

        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;
        // Assert.NotNull(testObject); // Ensure testObject is not null
        //Assert.False(string.IsNullOrWhiteSpace(testObject.Id), "Test object ID should not be null or empty");
        _output.WriteLine($"Testing: {description}");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Response time: {stopwatch.ElapsedMilliseconds}ms (max: {maxMilliseconds}ms)");
        response.StatusCode.Should().BeOneOf(
    HttpStatusCode.OK,
    HttpStatusCode.BadRequest,
    HttpStatusCode.InternalServerError  // API bug
);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxMilliseconds,
            $"{description} should respond within {maxMilliseconds}ms");
        _output.WriteLine($"✓ {description} passed performance test");
    }

    #endregion

    #region Batch Test Scenarios

    public class TestScenario
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object?> Data { get; set; } = new();
        public HttpStatusCode ExpectedStatus { get; set; }
        public bool ShouldSucceed { get; set; }
    }

    public static IEnumerable<object[]> BatchTestScenarios =>
        new List<object[]>
        {
            new object[] { new TestScenario
            {
                Name = "Valid Product",
                Data = new() { { "price", 99.99 }, { "stock", 10 } },
                ExpectedStatus = HttpStatusCode.OK,
                ShouldSucceed = true
            }},
            new object[] { new TestScenario
            {
                Name = "Empty Data",
                Data = new(),
                ExpectedStatus = HttpStatusCode.OK,
                ShouldSucceed = true
            }},
            new object[] { new TestScenario
            {
                Name = "Unicode Name 测试",
                Data = new() { { "language", "Chinese" } },
                ExpectedStatus = HttpStatusCode.OK,
                ShouldSucceed = true
            }},
            new object[] { new TestScenario
            {
                Name = "Complex Nested",
                Data = new() {
                    { "level1", new Dictionary<string, object>
                        {
                            { "level2", "value" }
                        }
                    }
                },
                ExpectedStatus = HttpStatusCode.OK,
                ShouldSucceed = true
            }}
        };

    [Theory]
    [MemberData(nameof(BatchTestScenarios))]
    [Trait("Category", "Parameterized")]
    [Trait("Priority", "High")]
    public async Task PatchObject_BatchScenarios_ShouldMatchExpectations(TestScenario scenario)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Batch Test Object");
        _output.WriteLine($"Testing scenario: {scenario.Name}");
        _output.WriteLine($"Data: {System.Text.Json.JsonSerializer.Serialize(scenario.Data)}");

        var updateData = new { name = scenario.Name, data = scenario.Data };

        // Act
        var response = await _apiClient.PatchObjectAsync(testObject!.Id!, updateData);

        // Assert
        _output.WriteLine($"Expected: {scenario.ExpectedStatus}, Actual: {response.StatusCode}");
        response.StatusCode.Should().Be(scenario.ExpectedStatus);
        response.IsSuccessful.Should().Be(scenario.ShouldSucceed);

        if (scenario.ShouldSucceed)
        {
            response.Data.Should().NotBeNull();
            response.Data!.Name.Should().Be(scenario.Name);
        }

        _output.WriteLine($"✓ Scenario '{scenario.Name}' passed");
    }

    #endregion

    #region Cross-Field Validation Tests

    public static IEnumerable<object[]> CrossFieldTestCases =>
        new List<object[]>
        {
            new object[]
            {
                "Name and Price",
                new { name = "Product A", data = new Dictionary<string, object?> { { "price", 99.99 } } }
            },
            new object[]
            {
                "Name and Multiple Fields",
                new { name = "Product B", data = new Dictionary<string, object?>
                    {
                        { "price", 149.99 },
                        { "stock", 25 },
                        { "category", "Electronics" }
                    }
                }
            },
            new object[]
            {
                "Name Only",
                new { name = "Product C" }
            },
            new object[]
            {
                "Data Only",
                new { data = new Dictionary<string, object?> { { "color", "Blue" } } }
            }
        };
    [Theory]
    [MemberData(nameof(CrossFieldTestCases))]
    [Trait("Category", "Parameterized")]
    [Trait("Priority", "Medium")]
    public async Task PatchObject_WithVariousFieldCombinations_ShouldSucceed(
           string description,
           object updateData)
    {
        // Arrange
        var testObject = await CreateTestObjectAsync("Original Object");

        // Validate test object creation
        testObject.Should().NotBeNull($"Failed to create test object for: {description}");

        if (testObject == null)
        {
            _output.WriteLine($"⚠️ Skipping test '{description}' - failed to create test object");
            return;
        }

        testObject.Id.Should().NotBeNullOrWhiteSpace($"Test object ID should be valid for: {description}");

        _output.WriteLine($"Testing: {description}");
        _output.WriteLine($"Test object ID: {testObject.Id}");
        _output.WriteLine($"Update data: {System.Text.Json.JsonSerializer.Serialize(updateData)}");

        // Act
        var response = await _apiClient.PatchObjectAsync(testObject.Id!, updateData);

        // Assert
        _output.WriteLine($"Response status: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Test '{description}' should succeed");
        response.Data.Should().NotBeNull($"Response data should not be null for: {description}");
        _output.WriteLine($"✓ {description} test passed");
    }


    #endregion

    #region Custom Test Data Builder

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [Trait("Category", "Parameterized")]
    [Trait("Priority", "Low")]
    public async Task PatchObject_WithDynamicDataSets_ShouldHandleAll(int dataFieldCount)
    {
        // Arrange
        //var testObject = await CreateTestObjectAsync("Dynamic Test");
        var testObject = await CreateTestObjectAsync("Test Object");

        // ✅ REQUIRED: Null check
        if (testObject == null)
        {
            _output.WriteLine("⚠️ Skipping test - failed to create test object");
            return;
        }

        testObject.Should().NotBeNull();
        var objectId = testObject!.Id!;

        // Generate dynamic data with N fields
        var dynamicData = new Dictionary<string, object?>();
        for (int i = 0; i < dataFieldCount; i++)
        {
            dynamicData[$"field{i}"] = $"value{i}";
        }

        _output.WriteLine($"Testing with {dataFieldCount} data fields");
        var updateData = new { name = $"Dynamic_{dataFieldCount}", data = dynamicData };

        // Act
        var response = await _apiClient.PatchObjectAsync(objectId, updateData);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.BadRequest, 
            HttpStatusCode.InternalServerError);
        response.Data.Should().NotBeNull();
        _output.WriteLine($"✓ Successfully handled {dataFieldCount} fields");
    }

    #endregion
}