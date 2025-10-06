# Contributing to RestfulApi.Dev Test Suite

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to this project.

## 🤝 Ways to Contribute

- 🐛 **Report bugs** - Found an issue? Let us know
- ✨ **Suggest features** - Have an idea? Share it
- 📝 **Improve documentation** - Help make our docs better
- 🧪 **Add tests** - More coverage is always welcome
- 🔧 **Fix issues** - Check our issue tracker

## 📋 Before You Start

1. **Check existing issues** - Someone may already be working on it
2. **Read the documentation** - Understand the project structure
3. **Set up your environment** - Follow the setup instructions
4. **Run existing tests** - Ensure everything works

## 🚀 Getting Started

### 1. Fork and Clone

```bash
# Fork the repository on GitHub, then clone your fork
git clone https://github.com/YOUR-USERNAME/restful-api-dev-tests.git
cd restful-api-dev-tests

# Add upstream remote
git remote add upstream https://github.com/ORIGINAL-OWNER/restful-api-dev-tests.git
```

### 2. Create a Branch

```bash
# Create a feature branch
git checkout -b feature/your-feature-name

# Or for bug fixes
git checkout -b fix/issue-description
```

### 3. Make Your Changes

Follow the coding standards and project structure outlined below.

## 📝 Coding Standards

### C# Style Guide

```csharp
// ✅ Good: Descriptive names, proper formatting
public async Task<RestResponse> PatchObjectAsync(string objectId, object updateData)
{
    var request = new RestRequest($"/objects/{objectId}", Method.Patch);
    request.AddJsonBody(updateData);
    return await _client.ExecuteAsync(request);
}

// ❌ Bad: Poor naming, no async suffix
public Task<RestResponse> patch(string id, object data)
{
    var r = new RestRequest($"/objects/{id}", Method.Patch);
    r.AddJsonBody(data);
    return _client.ExecuteAsync(r);
}
```

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `ApiClient`, `TestDataBuilder` |
| Methods | PascalCase | `GetObjectAsync`, `CreateTestObject` |
| Parameters | camelCase | `objectId`, `updateData` |
| Private fields | _camelCase | `_apiClient`, `_baseUrl` |
| Constants | PascalCase | `MaxRetries`, `DefaultTimeout` |

### Test Naming

```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
[Fact]
public async Task PatchObject_WithValidData_ShouldSucceed()
{
    // Test implementation
}

[Fact]
public async Task PatchObject_WithInvalidId_ShouldReturn404()
{
    // Test implementation
}
```

## 🧪 Adding New Tests

### Test Structure

```csharp
[Fact]
[Trait("Category", "YourCategory")]  // HappyPath, Negative, EdgeCase, Performance
[Trait("Priority", "High")]           // High, Medium, Low
public async Task PatchObject_YourScenario_ExpectedOutcome()
{
    // Arrange - Set up test data
    var testObject = await CreateTestObjectAsync("Test Name");
    var updateData = new { name = "Updated Name" };
    
    // Act - Execute the test
    var response = await _apiClient.PatchObjectAsync(testObject.Id!, updateData);
    
    // Assert - Verify the outcome
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Data!.Name.Should().Be("Updated Name");
}
```

### Test Categories

Use appropriate traits for your tests:

- **HappyPath** - Successful scenarios, expected behavior
- **Negative** - Error cases, invalid inputs
- **EdgeCase** - Boundary conditions, unusual scenarios
- **Performance** - Speed and efficiency tests
- **DataConsistency** - Data persistence and integrity
- **TypeHandling** - Different data types
- **Headers** - HTTP header validation

### Test Checklist

Before submitting a test:

- [ ] Test has descriptive name following naming convention
- [ ] Test has appropriate `[Trait]` attributes
- [ ] Test follows Arrange-Act-Assert pattern
- [ ] Test uses FluentAssertions for assertions
- [ ] Test cleans up created resources
- [ ] Test is independent (doesn't depend on other tests)
- [ ] Test handles async operations properly
- [ ] Test has meaningful assertion messages

## 📚 Documentation

### Code Comments

```csharp
/// 
/// Partially updates an object using PATCH request
/// 
/// The ID of the object to update
/// Object containing fields to update
/// Response containing updated object
public async Task<RestResponse> PatchObjectAsync(string objectId, object updateData)
{
    // Implementation
}
```

### Markdown Documentation

- Use clear headings and subheadings
- Include code examples where applicable
- Add tables for structured information
- Use emojis sparingly for visual appeal
- Keep line length reasonable (80-120 chars)

## 🔧 Development Workflow

### 1. Write Code

```bash
# Make your changes
code .
```

### 2. Run Tests Locally

```bash
# Run all tests
dotnet test

# Run specific category
dotnet test --filter Category=HappyPath

# Run your new test
dotnet test --filter "FullyQualifiedName~YourTestName"
```

### 3. Check Code Quality

```bash
# Build in Release mode
dotnet build --configuration Release

# Check for warnings
dotnet build /warnaserror
```

### 4. Update Documentation

If your changes affect:
- API behavior → Update OBSERVATIONS.md
- Setup process → Update README.md
- CI/CD → Update CI-PIPELINE.md

## 📤 Submitting Changes

### 1. Commit Your Changes

```bash
# Stage your changes
git add .

# Commit with descriptive message
git commit -m "Add: Test for PATCH with Unicode characters

- Added test for Unicode name updates
- Includes Chinese, Arabic, Russian, and emoji characters
- Verifies proper encoding handling
- Closes #123"
```

### Commit Message Guidelines

**Format:**
```
Type: Brief description (50 chars max)

- Detailed explanation point 1
- Detailed explanation point 2
- Reference to issue (if applicable)
```

**Types:**
- `Add:` New feature or test
- `Fix:` Bug fix
- `Update:` Modification to existing code
- `Refactor:` Code restructuring
- `Docs:` Documentation changes
- `Style:` Formatting, no code change
- `Test:` Adding or updating tests
- `Chore:` Maintenance tasks

**Examples:**

✅ Good:
```
Add: Test for empty request body handling

- Tests PATCH endpoint with empty JSON object
- Verifies API handles gracefully
- Adds corresponding documentation
- Fixes #42
```

❌ Bad:
```
fixed stuff
```

### 2. Push to Your Fork

```bash
git push origin feature/your-feature-name
```

### 3. Create Pull Request

1. Go to your fork on GitHub
2. Click "New Pull Request"
3. Select your branch
4. Fill in the PR template:

```markdown
## Description
Brief description of your changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Test addition

## Testing
Describe how you tested your changes

## Checklist
- [ ] Code follows project style guidelines
- [ ] Tests pass locally
- [ ] Documentation updated
- [ ] Commit messages are clear
- [ ] Branch is up to date with main
```

### 4. Code Review Process

- Maintainers will review your PR
- Address any feedback or requested changes
- Once approved, your PR will be merged

## 🐛 Reporting Issues

### Bug Report Template

```markdown
**Describe the bug**
A clear description of what the bug is

**To Reproduce**
Steps to reproduce the behavior:
1. Run command '...'
2. Execute test '...'
3. See error

**Expected behavior**
What you expected to happen

**Actual behavior**
What actually happened

**Environment:**
- OS: [e.g., Windows 11, macOS 14, Ubuntu 22.04]
- .NET Version: [e.g., 8.0.1]
- Project Version: [e.g., commit SHA or tag]

**Additional context**
Any other relevant information
```

### Feature Request Template

```markdown
**Feature Description**
Clear description of the feature

**Use Case**
Why is this feature needed?

**Proposed Solution**
How you envision this working

**Alternatives Considered**
Other approaches you've thought about

**Additional Context**
Any other relevant information
```

## 🧪 Test Coverage Guidelines

### Current Coverage by Category

| Category | Tests | Target |
|----------|-------|--------|
| Happy Path | 5 | Good |
| Negative Tests | 5 | Good |
| Edge Cases | 6 | Good |
| Performance | 2 | Could improve |
| Data Consistency | 3 | Good |

### Areas Needing More Tests

1. **Authentication scenarios** (if API adds auth)
2. **Rate limiting** (when implemented)
3. **Concurrent requests** (thread safety)
4. **Large payload handling** (stress testing)
5. **Network failure scenarios** (resilience)

## 🎯 Priority Guidelines

When adding tests or fixing issues, use these priorities:

### High Priority
- Blocking bugs
- Security issues
- Core functionality tests
- Critical performance issues

### Medium Priority
- Non-blocking bugs
- Feature enhancements
- Additional edge case tests
- Documentation improvements

### Low Priority
- Nice-to-have features
- Code refactoring
- Style improvements
- Minor documentation updates

## 🔍 Code Review Guidelines

### As a Reviewer

- Be constructive and respectful
- Explain your reasoning
- Suggest improvements, don't just criticize
- Approve when appropriate
- Test the changes if possible

### Example Review Comments

✅ Good:
```
Consider using FluentAssertions here for better readability:
response.Data!.Name.Should().Be("Expected Name");

This provides clearer error messages when tests fail.
```

❌ Bad:
```
This is wrong, fix it.
```

## 🛠️ Development Tools

### Recommended Extensions

**Visual Studio 2022:**
- ReSharper (code analysis)
- CodeMaid (cleanup)
- Test Explorer

**VS Code:**
- C# (Microsoft)
- C# Extensions
- Test Explorer UI

**JetBrains Rider:**
- Built-in tools are sufficient

### Useful Commands

```bash
# Format code
dotnet format

# Run code analysis
dotnet build /p:RunAnalyzersDuringBuild=true

# Watch tests
dotnet watch test

# Generate coverage
dotnet test /p:CollectCoverage=true
```

## 📊 Performance Considerations

When adding new tests:

- **Minimize test setup time** - Reuse test objects when possible
- **Use async properly** - Don't block on async calls
- **Clean up resources** - Delete created test data
- **Avoid Thread.Sleep** - Use proper async waits
- **Test in parallel** - Ensure tests are independent

## 🔐 Security Considerations

- **Never commit API keys** - Use environment variables
- **Sanitize test data** - Don't use real personal information
- **Handle sensitive data** - Follow security best practices
- **Test security scenarios** - Include auth/authz tests if applicable

## 📋 Pull Request Checklist

Before submitting your PR, ensure:

- [ ] Code compiles without errors
- [ ] All existing tests pass
- [ ] New tests are added (if applicable)
- [ ] Code follows style guidelines
- [ ] Documentation is updated
- [ ] Commit messages are clear
- [ ] Branch is up to date with main
- [ ] PR description is complete
- [ ] Self-review completed
- [ ] No sensitive data included

## 🎓 Learning Resources

### .NET Testing
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Microsoft Testing Guidelines](https://docs.microsoft.com/en-us/dotnet/core/testing/)

### API Testing
- [REST API Best Practices](https://restfulapi.net/)
- [HTTP Status Codes](https://httpstatuses.com/)
- [RestSharp Documentation](https://restsharp.dev/)

### GitHub Actions
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Workflow Syntax](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions)

## 💬 Communication

### Where to Ask Questions

- **GitHub Discussions** - General questions and ideas
- **GitHub Issues** - Bug reports and feature requests
- **Pull Request Comments** - Specific code questions

### Response Time

- Bug reports: 2-3 business days
- Feature requests: 1 week
- Pull requests: 3-5 business days

## 🏆 Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md
- Mentioned in release notes
- Credited in commit history

## 📄 License

By contributing, you agree that your contributions will be licensed under the same license as the project (MIT License).

## ❓ Questions?

If you have questions not covered here:

1. Check existing documentation
2. Search closed issues
3. Open a new discussion
4. Contact maintainers

---

**Thank you for contributing! 🎉**

Your contributions help make this project better for everyone.