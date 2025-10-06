# CI/CD Pipeline Documentation

## Overview

This project uses **GitHub Actions** for Continuous Integration (CI) to automatically run API tests on every code change. The pipeline ensures code quality and API functionality are maintained throughout the development lifecycle.

## Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    GitHub Repository                         │
│  (Push/PR to main or develop branch)                        │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              GitHub Actions Workflow                         │
│                (api-tests.yml)                               │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┴──────────────┐
        ▼                           ▼
┌──────────────┐          ┌──────────────────┐
│ Checkout     │          │ Setup .NET 8.0   │
│ Repository   │          │ SDK              │
└──────┬───────┘          └────────┬─────────┘
       │                           │
       └──────────┬────────────────┘
                  ▼
       ┌──────────────────┐
       │ Restore Packages │
       └────────┬─────────┘
                ▼
       ┌──────────────────┐
       │ Build Solution   │
       └────────┬─────────┘
                ▼
       ┌──────────────────┐
       │ Run API Tests    │
       └────────┬─────────┘
                │
    ┌───────────┴──────────────┐
    ▼                          ▼
┌─────────────┐      ┌──────────────────┐
│ Generate    │      │ Upload Test      │
│ Test Report │      │ Results/Artifacts│
└─────────────┘      └──────────────────┘
                              │
                              ▼
                   ┌─────────────────────┐
                   │ Report Status       │
                   │ ✅ Success / ❌ Fail│
                   └─────────────────────┘
```

## Workflow File Location

```
.github/workflows/api-tests.yml
```

## Trigger Events

The pipeline is triggered automatically on:

### 1. **Push Events**
- Branches: `main`, `develop`
- Triggered when commits are pushed to these branches

### 2. **Pull Request Events**
- Target Branch: `main`
- Triggered when PR is opened, synchronized, or reopened

### 3. **Manual Trigger**
- Via GitHub UI using "Run workflow" button
- Useful for on-demand testing

## Pipeline Stages

### Stage 1: Environment Setup

**Purpose**: Prepare the build environment

**Steps**:
1. **Checkout Repository**
   - Uses: `actions/checkout@v4`
   - Fetches complete git history
   - Required for building and testing

2. **Setup .NET SDK**
   - Uses: `actions/setup-dotnet@v4`
   - Version: .NET 8.0.x
   - Installs required SDK for building C# projects

3. **Display .NET Version**
   - Verification step
   - Shows installed .NET version in logs

**Expected Duration**: ~15-20 seconds

---

### Stage 2: Build Process

**Purpose**: Compile the test project

**Steps**:
1. **Restore Dependencies**
   ```bash
   dotnet restore RestfulApiDevTests/RestfulApiDevTests.csproj
   ```
   - Downloads NuGet packages
   - RestSharp, xUnit, FluentAssertions, etc.

2. **Build Project**
   ```bash
   dotnet build --configuration Release --no-restore
   ```
   - Compiles C# code
   - Uses Release configuration for optimization
   - Skips restore (already done)

**Expected Duration**: ~10-15 seconds

**Failure Scenarios**:
- Missing dependencies
- Compilation errors
- Syntax errors in code

---

### Stage 3: Test Execution

**Purpose**: Run all automated API tests

**Command**:
```bash
dotnet test \
  --configuration Release \
  --no-build \
  --verbosity normal \
  --logger "trx;LogFileName=test-results.trx" \
  --logger "html;LogFileName=test-results.html" \
  --results-directory ./TestResults
```

**Parameters**:
- `--configuration Release`: Use optimized build
- `--no-build`: Skip rebuild (already built)
- `--verbosity normal`: Standard output level
- `--logger "trx"`: Generate XML test results
- `--logger "html"`: Generate HTML report
- `--results-directory`: Output location

**Test Categories Executed**:
- ✅ Happy Path (5 tests)
- ❌ Negative Tests (5 tests)
- 🔍 Edge Cases (6 tests)
- ⚡ Performance Tests (2 tests)
- 📋 Headers Tests (1 test)
- 🔄 Data Consistency (3 tests)
- 🔢 Type Handling (1 test)

**Expected Duration**: ~25-45 seconds

**Output Files**:
- `TestResults/test-results.trx` - XML format
- `TestResults/test-results.html` - HTML format

---

### Stage 4: Results Processing

**Purpose**: Publish and archive test results

**Steps**:
1. **Publish Test Results**
   - Uses: `EnricoMi/publish-unit-test-result-action@v2`
   - Parses TRX files
   - Creates PR comments with results
   - Generates check runs

2. **Upload Artifacts**
   - Uses: `actions/upload-artifact@v4`
   - Archives test results for 30 days
   - Accessible via Actions UI

3. **Generate Summary**
   - Creates markdown summary
   - Displays in workflow run page
   - Includes:
     - Test configuration
     - API endpoint tested
     - .NET version
     - Timestamp
     - Commit SHA

**Expected Duration**: ~5-10 seconds

---

### Stage 5: Status Reporting

**Purpose**: Report final status and handle failures

**Steps**:
1. **Verify Test Results**
   - Parses TRX file
   - Checks for failed tests
   - Exits with code 1 if failures found

2. **Report Status Job**
   - Separate job for status reporting
   - Runs regardless of test outcome
   - Sends notifications (success/failure)

**Outcomes**:
- ✅ **Success**: All tests passed
- ❌ **Failure**: One or more tests failed
- ⚠️ **Warning**: Tests passed but with warnings

---

## Pipeline Configuration

### Environment Variables

```yaml
env:
  DOTNET_VERSION: '8.0.x'
  TEST_PROJECT_PATH: 'RestfulApiDevTests/RestfulApiDevTests.csproj'
```

### Job Configuration

```yaml
jobs:
  test:
    name: Run API Tests
    runs-on: ubuntu-latest  # Linux environment
```

**Why Ubuntu?**
- Faster startup than Windows
- More cost-effective
- .NET Core is cross-platform
- Sufficient for API testing

---

## Viewing Pipeline Results

### Method 1: GitHub Actions Tab

1. Navigate to repository on GitHub
2. Click **Actions** tab
3. Select workflow run
4. View detailed logs and results

### Method 2: Pull Request Checks

- PR shows check status automatically
- ✅ Green checkmark = All tests passed
- ❌ Red X = Tests failed
- 🟡 Yellow dot = Running

### Method 3: Email Notifications

- Configured in GitHub settings
- Sends email on workflow failure
- Includes link to failed run

### Method 4: Artifacts Download

1. Go to workflow run
2. Scroll to "Artifacts" section
3. Download `test-results` artifact
4. Extract and view HTML report

---

## Reading Test Results

### TRX Format (XML)

```xml

  
    
  

```

### HTML Report

- Visual representation
- Color-coded results
- Detailed error messages
- Execution times
- Stack traces for failures

### Summary Example

```
🧪 API Test Execution Summary

Test Configuration
- API Under Test: restful-api.dev
- Endpoint Tested: PATCH /objects/{id}
- .NET Version: 8.0.x
- Run Date: 2025-10-04 10:30:00 UTC

Results: ✅ 23 passed, ❌ 2 failed, ⏭️ 0 skipped
Duration: 42.5 seconds
```

---

## Handling Failures

### When Tests Fail

1. **Review Failure Details**
   - Click on failed job
   - Expand failed test step
   - Read error message

2. **Download Artifacts**
   - Get detailed HTML report
   - Review stack traces
   - Check request/response data

3. **Local Reproduction**
   ```bash
   git checkout <commit-sha>
   dotnet test --filter "FullyQualifiedName~FailedTestName"
   ```

4. **Fix and Rerun**
   - Fix the issue
   - Commit changes
   - Pipeline runs automatically

### Common Failure Reasons

| Issue | Cause | Solution |
|-------|-------|----------|
| Build Failure | Compilation error | Fix syntax errors |
| Test Timeout | API unavailable | Check API status |
| Assertion Failure | API behavior changed | Update tests or expectations |
| Network Error | Connection issue | Retry or check network |

---

## Pipeline Optimization

### Current Optimizations

1. **Caching** (can be added):
   ```yaml
   - uses: actions/cache@v3
     with:
       path: ~/.nuget/packages
       key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
   ```

2. **Parallel Execution**:
   - Tests run in parallel by xUnit
   - Faster overall execution

3. **Conditional Steps**:
   - `if: always()` for result upload
   - `continue-on-error: true` for test step
   - Ensures artifacts are saved even on failure

### Future Enhancements

1. **Test Sharding**
   - Split tests across multiple jobs
   - Reduce total runtime

2. **Coverage Reports**
   - Add code coverage collection
   - Generate coverage badges

3. **Performance Tracking**
   - Track API response times over time
   - Alert on degradation

4. **Scheduled Runs**
   ```yaml
   on:
     schedule:
       - cron: '0 0 * * *'  # Daily at midnight
   ```

---

## Security Considerations

### API Keys
- **Current**: No API key required (public API)
- **Future**: Use GitHub Secrets for sensitive data
  ```yaml
  env:
    API_KEY: ${{ secrets.API_KEY }}
  ```

### Permissions

```yaml
permissions:
  contents: read      # Read repository content
  checks: write       # Write check runs
  pull-requests: write # Comment on PRs
```

---

## Troubleshooting

### Pipeline Not Triggering

**Check**:
1. Workflow file syntax
2. Branch name matches trigger
3. Workflow is enabled in settings

**Fix**:
```bash
# Validate YAML
yamllint .github/workflows/api-tests.yml
```

### Tests Pass Locally, Fail in CI

**Common Causes**:
- Environment differences
- Timing issues
- Network conditions

**Debug**:
```yaml
- name: Debug Environment
  run: |
    echo "Runner OS: $RUNNER_OS"
    echo ".NET Version: $(dotnet --version)"
    printenv
```

### Slow Pipeline

**Optimization Tips**:
1. Enable NuGet caching
2. Use `--no-restore` and `--no-build` flags
3. Reduce test verbosity
4. Parallelize test execution

---

## Cost Considerations

### GitHub Actions Minutes

- **Free Tier**: 2,000 minutes/month
- **This Pipeline**: ~1-2 minutes per run
- **Estimated Runs**: ~1,000 per month possible

### Storage

- **Artifacts**: 500MB free
- **Retention**: 30 days
- **Per Run**: ~1-5MB

---

## Best Practices

### ✅ Do

- Keep workflow files simple
- Use specific action versions (not `@latest`)
- Add meaningful job/step names
- Generate test reports
- Upload artifacts for debugging
- Use environment variables for configuration

### ❌ Don't

- Hardcode secrets in workflow
- Use outdated actions
- Skip error handling
- Ignore failed tests
- Run unnecessary steps
- Over-complicate the pipeline

---

## Monitoring and Alerts

### GitHub Status Badge

Add to README.md:
```markdown
![API Tests](https://github.com/username/repo/actions/workflows/api-tests.yml/badge.svg)
```

### Slack/Discord Integration (Optional)

Add notification step:
```yaml
- name: Notify on Failure
  if: failure()
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

---

## Metrics and KPIs

### Track These Metrics

1. **Test Success Rate**
   - Target: > 95%
   - Current: ~92%

2. **Pipeline Duration**
   - Target: < 3 minutes
   - Current: ~1.5-2 minutes

3. **Test Coverage**
   - Target: > 80%
   - Current: Not measured (can be added)

4. **Flaky Test Rate**
   - Target: < 5%
   - Track tests that fail intermittently

---

## Local Pipeline Simulation

To test the pipeline locally before pushing:

```bash
# Install act (GitHub Actions local runner)
# macOS
brew install act

# Linux
curl https://raw.githubusercontent.com/nektos/act/master/install.sh | sudo bash

# Windows
choco install act-cli

# Run the workflow locally
act -j test

# Run with secrets
act -j test --secret-file .secrets
```

---

## Pipeline Maintenance

### Monthly Tasks

- [ ] Review and update action versions
- [ ] Check for deprecated features
- [ ] Review test execution times
- [ ] Clean up old artifacts
- [ ] Update documentation

### Quarterly Tasks

- [ ] Audit pipeline security
- [ ] Review and optimize caching strategy
- [ ] Update .NET SDK version if needed
- [ ] Review test coverage
- [ ] Update dependencies

---

## Advanced Configuration

### Matrix Strategy (for multiple versions)

```yaml
strategy:
  matrix:
    dotnet-version: ['6.0.x', '7.0.x', '8.0.x']
    os: [ubuntu-latest, windows-latest, macos-latest]

steps:
  - uses: actions/setup-dotnet@v4
    with:
      dotnet-version: ${{ matrix.dotnet-version }}
```

### Conditional Execution

```yaml
- name: Run Integration Tests
  if: github.ref == 'refs/heads/main'
  run: dotnet test --filter Category=Integration
```

### Reusable Workflows

Create `.github/workflows/reusable-test.yml`:
```yaml
on:
  workflow_call:
    inputs:
      dotnet-version:
        required: true
        type: string

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ inputs.dotnet-version }}
```

---

## Comparison with Other CI Tools

| Feature | GitHub Actions | Jenkins | Azure DevOps | CircleCI |
|---------|---------------|---------|--------------|----------|
| Setup Complexity | ⭐⭐⭐⭐⭐ Low | ⭐⭐ High | ⭐⭐⭐ Medium | ⭐⭐⭐⭐ Low |
| Integration | Native GitHub | Plugin-based | Azure services | Good |
| Free Tier | 2000 min/month | Self-hosted | 1800 min/month | 6000 min/month |
| Configuration | YAML | Groovy | YAML | YAML |
| Community | Large | Very Large | Large | Medium |
| .NET Support | ✅ Excellent | ✅ Good | ✅ Excellent | ✅ Good |

**Recommendation**: GitHub Actions is ideal for this project due to:
- Native GitHub integration
- Simple YAML configuration
- Sufficient free tier
- Excellent .NET support
- No external setup required

---

## Conclusion

This CI pipeline provides:

✅ **Automated Testing** - Every commit is tested  
✅ **Fast Feedback** - Results in 1-2 minutes  
✅ **Detailed Reports** - HTML and XML reports  
✅ **Easy Debugging** - Artifacts preserved  
✅ **Quality Gates** - PRs blocked if tests fail  

**Next Steps**:
1. Push code to GitHub repository
2. Verify workflow runs automatically
3. Review test results in Actions tab
4. Monitor for failures and optimize

For questions or issues with the pipeline, refer to:
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET Testing Documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- Project maintainer