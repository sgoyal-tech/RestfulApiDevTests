# Quick Start Guide

Get up and running with the RestfulApi.Dev API Test Suite in under 5 minutes!

## Prerequisites Check ✓

Before starting, ensure you have:

```bash
# Check .NET SDK
dotnet --version
# Should show: 8.0.x or later

# Check Git
git --version
# Should show: 2.x.x or later
```

If you don't have .NET SDK installed:
- **Windows**: Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **macOS**: `brew install dotnet`
- **Linux**: See [Microsoft docs](https://docs.microsoft.com/dotnet/core/install/linux)

---

## 🚀 Getting Started (3 Steps)

### Step 1: Clone the Repository

```bash
# Clone the repo
git clone https://github.com/yourusername/restful-api-dev-tests.git

# Navigate to the directory
cd restful-api-dev-tests
```

### Step 2: Build the Project

```bash
# Restore dependencies and build
dotnet build RestfulApiDevTests/RestfulApiDevTests.csproj
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Step 3: Run the Tests

```bash
# Run all tests
dotnet test RestfulApiDevTests/RestfulApiDevTests.csproj
```

Expected output:
```
Starting test execution...
Passed!  - Failed:     0, Passed:    25, Skipped:     0, Total:    25
```

✅ **Done!** Your tests are now running.

---

## 📊 View Detailed Results

### Generate HTML Report

```bash
dotnet test RestfulApiDevTests/RestfulApiDevTests.csproj \
  --logger "html;LogFileName=test-results.html" \
  --results-directory ./TestResults
```

Open the report:
```bash
# macOS
open TestResults/test-results.html

# Windows
start TestResults/test-results.html

# Linux
xdg-open TestResults/test-results.html
```

---

## 🎯 Run Specific Test Categories

### Happy Path Tests Only
```bash
dotnet test --filter Category=HappyPath
```

### Negative Tests Only
```bash
dotnet test --filter Category=Negative
```

### Performance Tests Only
```bash
dotnet test --filter Category=Performance
```

### High Priority Tests Only
```bash
dotnet test --filter Priority=High
```

---

## 🐛 Troubleshooting

### Issue: "SDK not found"

**Solution**: Install .NET 8.0 SDK
```bash
# Verify installation
dotnet --list-sdks
```

### Issue: "RestSharp not found"

**Solution**: Restore NuGet packages
```bash
dotnet restore RestfulApiDevTests/RestfulApiDevTests.csproj
```

### Issue: Tests are failing

**Possible Causes**:
1. **API is down** - Check https://api.restful-api.dev/objects
2. **Network issues** - Verify internet connection
3. **Firewall** - Ensure HTTPS traffic is allowed

**Quick Fix**:
```bash
# Run tests with verbose output
dotnet test --verbosity detailed
```

### Issue: Build errors

**Solution**: Clean and rebuild
```bash
dotnet clean
dotnet build
```

---

## 🔧 Development Setup

### Using Visual Studio 2022

1. Open `RestfulApiDevTests.sln`
2. Build Solution (Ctrl+Shift+B)
3. Open Test Explorer (Ctrl+E, T)
4. Run All Tests

### Using VS Code

1. Install C# extension
2. Open project folder
3. Press F5 to run tests
4. View results in Test Explorer

### Using JetBrains Rider

1. Open solution file
2. Build → Build Solution
3. Unit Tests → Run All Tests
4. View results in Unit Tests panel

---

## 📝 First Test Run Checklist

After your first successful test run:

- [ ] All tests passed (25/25)
- [ ] Response times are acceptable (< 2s)
- [ ] HTML report generated successfully
- [ ] No build warnings
- [ ] API is accessible

---

## 🎓 Next Steps

Once you're up and running:

1. **Read the full README** - `README.md`
2. **Review test observations** - `OBSERVATIONS.md`
3. **Understand the CI pipeline** - `CI-PIPELINE.md`
4. **Explore the test code** - `Tests/PatchEndpointTests.cs`
5. **Try writing a new test** - Follow existing patterns

---

## 💡 Quick Tips

### Faster Test Runs
```bash
# Skip build if already compiled
dotnet test --no-build
```

### Watch Mode (Auto-run on changes)
```bash
dotnet watch test
```

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~PatchObject_WithNameOnly_ShouldUpdateSuccessfully"
```

### Parallel Execution
Tests run in parallel by default with xUnit. To run serially:
```bash
dotnet test -- xUnit.ParallelizeTestCollections=false
```

---

## 🌐 Useful Commands Reference

| Task | Command |
|------|---------|
| List all tests | `dotnet test --list-tests` |
| Run with coverage | `dotnet test /p:CollectCoverage=true` |
| Generate TRX report | `dotnet test --logger trx` |
| Run in Release mode | `dotnet test -c Release` |
| Clean build output | `dotnet clean` |
| Restore packages | `dotnet restore` |

---

## 🆘 Getting Help

If you encounter issues:

1. **Check the full README** - Most questions answered there
2. **Review logs** - Use `--verbosity detailed` flag
3. **Check API status** - Visit https://restful-api.dev
4. **Open an issue** - Include error messages and logs

---

## 🎉 Success!

You're now ready to:
- ✅ Run automated API tests
- ✅ Generate detailed reports
- ✅ Debug failing tests
- ✅ Contribute new tests

**Happy Testing! 🚀**