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


## 0.0.6
### Added
- Added SQL Server container configuration with database support in DevServerTests
- Added volume persistence for SQL Server container in tests
- Added connection string builder with `Encrypt` and `TrustServerCertificate` options in DevServerTests

### Updated/Fixed
- Changed SQL Server container lifetime from `Persistent` to `Session` in AppHost configuration
- Refactored DevServerTests:
  - Removed appsettings.Test.json configuration loading
  - Simplified SQL Server resource setup
  - Made DistributedApplication field private
  - Added 30-second delay before connection test
  - Updated connection string handling
- Cleaned up whitespace in AppHost.cs

### Removed
- Removed explicit password handling from DevServerTests
- Removed unnecessary `DisposeAsync` call in connection test


## 0.0.7
### Added
- Added test initialization and cleanup methods (`Setup` and `Cleanup`) in `RedisTests.cs` to manage `DistributedApplication` and Redis connection lifecycle
- Added `Aspire.Hosting` namespace import in `RedisTests.cs` for testing infrastructure

### Fixed/Updated
- Refactored `RedisTests.cs` to use class-level fields and setup/cleanup methods instead of per-test connection creation
- Removed redundant `RedisConnection()` helper method and replaced with centralized connection management
- Updated all test methods to use the shared connection from the setup method
- Improved resource cleanup with proper disposal of `DistributedApplication` and Redis connection in test teardown

### Removed
- Removed empty line in `AppHost.cs` for code cleanliness
- Removed per-test Redis connection creation logic from all test methods


## 0.0.8
### Fixed/Updated
- Improved error handling by replacing `ArgumentNullException` with more descriptive `InvalidOperationException` messages throughout the codebase
- Simplified Redis and SQL Server resource creation by removing unnecessary variable assignments (`IResourceBuilder<T>`)
- Cleaned up and removed commented-out Vault container configuration code
- Updated Redis tests to use more descriptive variable names (`redisConnection` instead of `connection`)
- Fixed variable naming consistency in tests (`app` → `application` where appropriate)

### Removed
- Removed unused `vaultToken` configuration and related Vault container setup
- Removed redundant variable assignments for Redis and SQL Server resources


## 0.0.9
### Added
- Added `Aspire.Hosting.Keycloak` package (version `9.4.0-preview.1.25378.8`) to project.
- Introduced `Microsoft.Extensions.Hosting` package (version `9.0.7`) to project dependencies.
- Declared a Redis password as a secure parameter using `builder.AddParameter("redisPassword", secret: true)`.
- Updated Redis resource to accept the secure Redis password and set container lifetime to session.

### Fixed/Updated
- Updated `Aspire.Hosting.AppHost` from version `9.3.0` to `9.4.0`.
- Updated `Aspire.Hosting.SqlServer` from version `9.3.1` to `9.4.0`.
- Upgraded `Microsoft.Extensions.Configuration.Json` from `9.0.6` to `9.0.7`.
- Changed exception type in `DevServerTests` when SQL connection string is missing, from `ArgumentNullException` to `InvalidOperationException`.

### Removed
- Removed `DotNetEnv` package (version `3.1.1`) and associated `#if DEBUG` block for loading `.env` file.
- Eliminated hardcoded Redis password retrieval from configuration.


## 0.0.10
### Added
- Introduced `Parameter__RedisPassword` and `Parameter__DevServerPassword` environment variables in GitHub Actions workflow.
- Integrated `AddUserSecrets<Program>` into configuration loading in `AppHost.cs`.

### Fixed/Updated
- Updated Redis and SQL Server configuration to use named parameters (`redisPassword`, `DevServerPassword`) instead of unnamed or differently named ones.
- Replaced `.AddParameter("sql-password", ...)` with `.AddParameter("DevServerPassword", ...)` to match usage.

### Removed
- Removed unused `Redis__Password`, `Hashicorp__Vault__Password`, and `DevServerPassword` environment variables from GitHub Actions.
- Removed `Redis:Password` from `appsettings.json`.