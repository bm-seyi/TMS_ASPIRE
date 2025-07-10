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