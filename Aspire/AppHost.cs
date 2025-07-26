using Aspire.Hosting;
using Aspire.Hosting.Redis;
using Microsoft.Extensions.Configuration;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

#if DEBUG
    string parentDirectory = Directory.GetParent(Environment.CurrentDirectory)?.FullName ?? throw new InvalidOperationException("Unable to retrieve Parent Directory Path");
    DotNetEnv.Env.Load(Path.Combine(parentDirectory, ".env"));
#endif

builder.Configuration.AddJsonFile("appsettings.json").AddEnvironmentVariables();

string redisPassword = builder.Configuration["Redis:Password"] ?? throw new InvalidOperationException("Redis Password could not be retrieved");

builder.AddRedis("redis-signalr")
    .WithImage("redis", "alpine3.21")
    .WithEnvironment("REDIS_PWD", redisPassword)
    .WithArgs("--requirepass", redisPassword, "--masterauth", redisPassword)
    .WithBindMount("./resources/redis/redis.conf", "/usr/local/etc/redis/redis.conf")
    .WithArgs("/usr/local/etc/redis/redis.conf");


IResourceBuilder<ParameterResource> password = builder.AddParameter("DevServerPassword", secret: true);

builder.AddSqlServer("DevServer", password, 1433)
    .WithLifetime(ContainerLifetime.Session)
    .WithImage("mssql/server", "2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("TZ", "Europe/London")
    .WithDataVolume("mssql_data");

await builder.Build().RunAsync();