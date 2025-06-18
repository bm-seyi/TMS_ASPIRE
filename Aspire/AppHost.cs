using Aspire.Hosting;
using Aspire.Hosting.Redis;
using Microsoft.Extensions.Configuration;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

#if DEBUG
    string parentDirectory = Directory.GetParent(Environment.CurrentDirectory)?.FullName ?? throw new ArgumentNullException(nameof(parentDirectory));
    DotNetEnv.Env.Load(Path.Combine(parentDirectory, ".env"));
#endif

builder.Configuration.AddJsonFile("appsettings.json").AddEnvironmentVariables();

string redisPassword = builder.Configuration["Redis:Password"] ?? throw new ArgumentNullException(nameof(redisPassword));

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis-signalr")
    .WithImage("redis", "alpine3.21")
    .WithEnvironment("REDIS_PWD", redisPassword)
    .WithArgs("--requirepass", redisPassword, "--masterauth", redisPassword)
    .WithBindMount("./resources/redis/redis.conf", "/usr/local/etc/redis/redis.conf")
    .WithArgs("/usr/local/etc/redis/redis.conf");


builder.Build().Run();