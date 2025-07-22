## 0.0.1
first development version


## 0.0.2
### Added
- Configuration for Hashicorp Vault token in `appsettings.json`
- Vault container resource configuration (commented out) in `AppHost.cs`
- Tests project to the solution file
- New test project with:
  - .NET 9.0 target framework
  - MSTest and Aspire.Hosting.Testing dependencies
  - Global usings for testing and Aspire hosting
  - Project reference to main Aspire project
- Redis integration test class with:
  - Health check test for Redis connection
  - Ping latency assertion
  - Distributed application testing infrastructure

### Updated
- Changed `Build().Run()` to `Build().RunAsync()` in `AppHost.cs` for async operation
- Added Redis password configuration check
- Solution file to include new Tests project


## 0.0.3
### Added
- Comprehensive Redis test suite including:
  - Basic connectivity test with ping measurement
  - Key-value set/get operations test
  - Multiple connection handling test
  - Concurrent access test with atomic increment verification
  - Expired key handling test
  - Invalid connection string test case
- Helper method `RedisConnection()` to simplify test setup
- Grpc.Core namespace import for additional functionality
- GitHub Actions workflow (`mstest-integration`) for:
  - Automated builds and tests on push/pull requests to master
  - .NET 8.0 environment setup
  - Solution restoration, building, and test execution

### Fixed/Updated
- Refactored existing Redis health test to use new helper method
- Improved test assertions with more descriptive messages
- Enhanced test coverage for various Redis scenarios


## 0.0.4
### Added
- Added SQL Server resource (`DevServer`) with persistent container lifetime and data volume in `AppHost.cs`
- Added parameter `DevServerPassword` for SQL Server authentication in `AppHost.cs` and `appsettings.json`
- Added environment variables for Redis and Hashicorp Vault passwords in GitHub workflow
- Added `appsettings.Test.json` to `.gitignore`
- Added new `DevServerTests` class with SQL Server connectivity tests
  - Includes test initialization/cleanup with DistributedApplication builder
  - Added connection validation test for SQL Server resource

### Fixed/Updated
- Updated GitHub workflow to use `ubuntu-latest` instead of `windows-latest` runner
- Updated .NET version from 8.0 to 9.0 in GitHub workflow
- Improved test configuration handling with `appsettings.Test.json` loading

### Removed
- Removed unused `using` statements (`Microsoft.Extensions.Logging`, `Microsoft.Extensions.Hosting`, etc.) from `RedisTests.cs`


## 0.0.5
### Fixed/Updated
- Changed `appsettings.Test.json` loading to be optional in `DevServerTests.cs` to prevent test failures when file doesn't exist