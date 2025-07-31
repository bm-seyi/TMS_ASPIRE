using Aspire.Hosting;
using Aspire.Hosting.Redis;
using Microsoft.Extensions.Configuration;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json").AddUserSecrets<Program>(optional: true).AddEnvironmentVariables();

IResourceBuilder<ParameterResource> redisPassword = builder.AddParameter("redisPassword", secret: true);

builder.AddRedis("redis-signalr", password: redisPassword)
    .WithLifetime(ContainerLifetime.Session)
    .WithImage("redis", "alpine3.21")
    .WithBindMount("./resources/redis/redis.conf", "/usr/local/etc/redis/redis.conf")
    .WithArgs("/usr/local/etc/redis/redis.conf");


IResourceBuilder<ParameterResource> devServerPassword = builder.AddParameter("DevServerPassword", secret: true);
builder.AddSqlServer("DevServer", devServerPassword, 1433)
    .WithLifetime(ContainerLifetime.Session)
    .WithImage("mssql/server", "2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("TZ", "Europe/London")
    .WithDataVolume("mssql_data");

await builder.Build().RunAsync();