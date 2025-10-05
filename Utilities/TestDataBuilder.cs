using RestfulApiDevTests.Models;

namespace RestfulApiDevTests.Utilities;

/// <summary>
/// Helper class for building test data objects
/// </summary>
public static class TestDataBuilder
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Creates a unique test object name with timestamp
    /// </summary>
    public static string CreateUniqueName(string prefix = "Test Object")
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var randomSuffix = _random.Next(1000, 9999);
        return $"{prefix}_{timestamp}_{randomSuffix}";
    }

    /// <summary>
    /// Creates a basic product data dictionary
    /// </summary>
    public static Dictionary<string, object?> CreateProductData(
        string? color = null,
        double? price = null,
        int? quantity = null)
    {
        var data = new Dictionary<string, object?>();

        if (color != null)
            data["color"] = color;

        if (price.HasValue)
            data["price"] = price.Value;

        if (quantity.HasValue)
            data["quantity"] = quantity.Value;

        return data;
    }

    /// <summary>
    /// Creates a device data dictionary
    /// </summary>
    public static Dictionary<string, object?> CreateDeviceData(
        string? brand = null,
        string? model = null,
        int? year = null,
        string? color = null)
    {
        var data = new Dictionary<string, object?>
        {
            ["brand"] = brand ?? "Apple",
            ["model"] = model ?? "iPhone 15",
            ["year"] = year ?? DateTime.UtcNow.Year,
            ["color"] = color ?? "Black"
        };

        return data;
    }

    /// <summary>
    /// Creates an update request with only name
    /// </summary>
    public static ApiObjectUpdate CreateNameOnlyUpdate(string name)
    {
        return new ApiObjectUpdate { Name = name };
    }

    /// <summary>
    /// Creates an update request with only data
    /// </summary>
    public static ApiObjectUpdate CreateDataOnlyUpdate(Dictionary<string, object?> data)
    {
        return new ApiObjectUpdate { Data = data };
    }

    /// <summary>
    /// Creates a complete update request
    /// </summary>
    public static ApiObjectUpdate CreateCompleteUpdate(
        string name,
        Dictionary<string, object?> data)
    {
        return new ApiObjectUpdate
        {
            Name = name,
            Data = data
        };
    }

    /// <summary>
    /// Creates random product data
    /// </summary>
    public static Dictionary<string, object?> CreateRandomProductData()
    {
        var colors = new[] { "Red", "Blue", "Green", "Black", "White", "Silver" };
        var categories = new[] { "Electronics", "Clothing", "Books", "Toys", "Sports" };

        return new Dictionary<string, object?>
        {
            ["color"] = colors[_random.Next(colors.Length)],
            ["price"] = Math.Round(_random.NextDouble() * 1000, 2),
            ["category"] = categories[_random.Next(categories.Length)],
            ["inStock"] = _random.Next(0, 2) == 1,
            ["quantity"] = _random.Next(0, 100)
        };
    }

    /// <summary>
    /// Creates data with special characters
    /// </summary>
    public static Dictionary<string, object?> CreateSpecialCharacterData()
    {
        return new Dictionary<string, object?>
        {
            ["field1"] = "Value with spaces",
            ["field2"] = "Value!@#$%^&*()",
            ["field3"] = "Value_with-dashes.and.dots",
            ["field4"] = "Value/with\\slashes"
        };
    }

    /// <summary>
    /// Creates data with various data types
    /// </summary>
    public static Dictionary<string, object?> CreateMixedTypeData()
    {
        return new Dictionary<string, object?>
        {
            ["stringField"] = "text value",
            ["intField"] = 42,
            ["doubleField"] = 3.14159,
            ["boolField"] = true,
            ["nullField"] = null,
            ["arrayField"] = new[] { 1, 2, 3 }
        };
    }

    /// <summary>
    /// Creates deeply nested data structure
    /// </summary>
    public static Dictionary<string, object?> CreateNestedData()
    {
        return new Dictionary<string, object?>
        {
            ["level1"] = new Dictionary<string, object?>
            {
                ["level2"] = new Dictionary<string, object?>
                {
                    ["level3"] = "deeply nested value",
                    ["level3_array"] = new[] { "item1", "item2" }
                }
            },
            ["simple_field"] = "simple value"
        };
    }

    /// <summary>
    /// Generates a very long string for testing
    /// </summary>
    public static string CreateLongString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Creates a name with Unicode characters
    /// </summary>
    public static string CreateUnicodeName()
    {
        return $"Unicode Test: 你好世界 مرحبا العالم Здравствуй мир 🚀🎉✨";
    }

    /// <summary>
    /// Creates an empty data dictionary
    /// </summary>
    public static Dictionary<string, object?> CreateEmptyData()
    {
        return new Dictionary<string, object?>();
    }

    /// <summary>
    /// Creates data with all null values
    /// </summary>
    public static Dictionary<string, object?> CreateNullValueData()
    {
        return new Dictionary<string, object?>
        {
            ["field1"] = null,
            ["field2"] = null,
            ["field3"] = null
        };
    }
}