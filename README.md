# RestfulApi.Dev API Test Automation Suite

A comprehensive .NET C# test automation framework for testing the [restful-api.dev](https://restful-api.dev) PATCH endpoint with CI/CD integration.

## 📋 Table of Contents
- [Overview](#overview)
- [API Documentation Understanding](#api-documentation-understanding)
- [Test Coverage](#test-coverage)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Installation & Setup](#installation--setup)
- [Running Tests](#running-tests)
- [CI/CD Pipeline](#cicd-pipeline)
- [Test Observations](#test-observations)

## 🎯 Overview

This project provides automated API testing for the restful-api.dev API, specifically focusing on the **PATCH /objects/{id}** endpoint. The tests are built using:
- **.NET 8.0** - Modern C# framework
- **xUnit** - Testing framework
- **RestSharp** - HTTP client library
- **FluentAssertions** - Assertion library for readable tests
- **GitHub Actions** - CI/CD automation

## 📚 API Documentation Understanding

### Base URL
```
https://api.restful-api.dev
```

### PATCH /objects/{id} Endpoint

**Purpose**: Partially update an existing object without requiring all fields

**Method**: PATCH

**URL Structure**: `https://api.restful-api.dev/objects/{id}`

**Request Headers**:
- `Content-Type: application/json`

**Request Body** (example):
```json
{
  "name": "Updated Name",
  "data": {
    "price": 299.99,
    "color": "Blue"
  }
}
```

**Success Response (200 OK)**:
```json
{
  "id": "6",
  "name": "Updated Name",
  "data": {
    "price": 299.99,
    "color": "Blue"
  },
  "updatedAt": "2024-10-04T10:30:00.000Z"
}
```

**Key Differences from PUT**:
- PATCH allows partial updates (only send fields to update)
- PUT requires complete resource representation
- PATCH is more flexible for selective field updates

## ✅ Test Coverage

The test suite covers the following scenarios:

### Happy Path Tests
1. **Successful partial update** - Update only name field
2. **Successful data attribute update** - Update nested data properties
3. **Complete object update** - Update all fields at once
4. **Multiple field updates** - Update name and data together

### Negative Tests
5. **Non-existent object ID** - Verify 404 error handling
6. **Invalid object ID format** - Test with non-numeric IDs
7. **Empty request body** - Verify behavior with no data
8. **Invalid JSON format** - Test malformed request bodies
9. **Null values handling** - Update fields with null values

### Edge Cases
10. **Very large object ID** - Test with extremely large numbers
11. **Special characters in name** - Unicode and special chars
12. **Empty data object** - Update with empty data field
13. **Response time validation** - Ensure acceptable performance
14. **Content-Type validation** - Verify proper headers

## 🔧 Prerequisites

- **.NET 8.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Git** ([Download](https://git-scm.com/downloads))
- **Visual Studio 2022** or **VS Code** (optional, for development)
- **Internet connection** (for API access)

### Verify .NET Installation
```bash
dotnet --version
# Should show 8.0.x or later
```

## 📁 Project Structure

```
RestfulApiDevTests/
├── .github/
│   └── workflows/
│       └── api-tests.yml          # GitHub Actions CI/CD pipeline
├── RestfulApiDevTests/
│   ├── Models/
│   │   ├── ApiObject.cs           # Data models
│   │   └── ApiError.cs
│   ├── Services/
│   │   └── ApiClient.cs           # HTTP client wrapper
│   ├── Tests/
│   │   ├── PatchEndpointTests.cs  # Main test class
│   │   └── TestBase.cs            # Base test class
│   ├── Utilities/
│   │   ├── TestDataBuilder.cs     # Test data generation
│   │   └── ApiHelpers.cs          # Helper methods
│   └── RestfulApiDevTests.csproj  # Project file
├── README.md                       # This file
├── OBSERVATIONS.md                 # Test findings & issues
└── .gitignore                      # Git ignore rules
```

## 🚀 Installation & Setup

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/restful-api-dev-tests.git
cd restful-api-dev-tests
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build the Project
```bash
dotnet build
```

### 4. Verify Setup
```bash
dotnet test --list-tests
```

## 🧪 Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Tests with Detailed Output
```bash
dotnet test --verbosity detailed
```

### Run Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run Specific Test Category
```bash
# Run only happy path tests
dotnet test --filter Category=HappyPath

# Run only negative tests
dotnet test --filter Category=Negative

# Run only edge cases
dotnet test --filter Category=EdgeCase
```

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~PatchEndpointTests.PatchObject_WithValidData_ShouldSucceed"
```

### Generate Test Report (HTML)
```bash
dotnet test --logger "html;LogFileName=test-results.html"
```

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

The project includes an automated CI/CD pipeline using GitHub Actions that:

1. **Triggers on**:
   - Every push to `main` or `develop` branches
   - Every pull request targeting `main` branch
   - Manual workflow dispatch

2. **Pipeline Steps**:
   - Checkout code
   - Setup .NET 8.0 SDK
   - Restore NuGet packages
   - Build solution
   - Run all tests
   - Generate test report
   - Upload test results as artifacts

3. **Notifications**:
   - ✅ Green check mark on success
   - ❌ Red X on failure
   - Test results available in Actions tab

### Viewing Pipeline Results

1. Go to your repository on GitHub
2. Click on **Actions** tab
3. Select the latest workflow run
4. View test results and download artifacts

### Local Pipeline Simulation

To simulate the CI pipeline locally:
```bash
# Clean build
dotnet clean

# Restore
dotnet restore

# Build
dotnet build --configuration Release

# Test
dotnet test --configuration Release --logger "trx;LogFileName=test-results.trx"
```

## 📊 Test Observations

Detailed findings from testing are documented in [OBSERVATIONS.md](OBSERVATIONS.md), including:

- API behavior patterns
- Response time analysis
- Error handling observations
- Data consistency checks
- Unexpected behaviors
- Recommendations for API improvements

### Quick Summary

**Strengths**:
- ✅ Consistent response format
- ✅ Clear error messages
- ✅ Fast response times (< 500ms)
- ✅ Proper HTTP status codes

**Issues Found**:
- ⚠️ Missing validation for some edge cases
- ⚠️ Inconsistent null value handling
- ⚠️ No rate limiting information

## 🛠️ Development

### Adding New Tests

1. Create test method in `PatchEndpointTests.cs`:
```csharp
[Fact]
[Trait("Category", "YourCategory")]
public async Task PatchObject_YourScenario_ShouldBehaveAsExpected()
{
    // Arrange
    var objectId = "6";
    var updateData = new { name = "New Name" };
    
    // Act
    var response = await _apiClient.PatchObjectAsync(objectId, updateData);
    
    // Assert
    response.Should().NotBeNull();
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

2. Run the new test:
```bash
dotnet test --filter "FullyQualifiedName~YourScenario"
```

### Debugging Tests

In Visual Studio:
1. Set breakpoints in test code
2. Right-click on test → **Debug Test(s)**

In VS Code:
1. Install C# extension
2. Set breakpoints
3. Use **Test Explorer** to debug

### Modifying API Client

Edit `Services/ApiClient.cs` to add new methods or modify behavior.

## 📦 Dependencies

- **RestSharp** (111.4.1) - REST API client
- **xUnit** (2.6.2) - Testing framework
- **FluentAssertions** (6.12.0) - Fluent assertion library
- **Microsoft.NET.Test.Sdk** (17.8.0) - Test SDK
- **xunit.runner.visualstudio** (2.5.4) - Test runner

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-test`)
3. Commit changes (`git commit -m 'Add amazing test'`)
4. Push to branch (`git push origin feature/amazing-test`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

## 🔗 Useful Links

- [restful-api.dev Official Site](https://restful-api.dev)
- [.NET Testing Documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [xUnit Documentation](https://xunit.net/)
- [RestSharp Documentation](https://restsharp.dev/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)

## 📧 Contact

For questions or issues, please open an issue on GitHub.

---

**Note**: This API is a free public service. Please use it responsibly and avoid creating excessive test data.