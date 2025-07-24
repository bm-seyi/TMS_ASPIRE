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
string vaultToken = builder.Configuration["Hashicorp:Vault:Token"] ?? throw new ArgumentNullException(nameof(vaultToken));

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis-signalr")
    .WithImage("redis", "alpine3.21")
    .WithEnvironment("REDIS_PWD", redisPassword)
    .WithArgs("--requirepass", redisPassword, "--masterauth", redisPassword)
    .WithBindMount("./resources/redis/redis.conf", "/usr/local/etc/redis/redis.conf")
    .WithArgs("/usr/local/etc/redis/redis.conf");


IResourceBuilder<ParameterResource> password = builder.AddParameter("DevServerPassword", secret: true);
IResourceBuilder<SqlServerServerResource> sqlServer = builder.AddSqlServer("DevServer", password, 1433)
    .WithLifetime(ContainerLifetime.Session)
    .WithImage("mssql/server", "2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("TZ", "Europe/London")
    .WithDataVolume("mssql_data");
    
/*
IResourceBuilder<ContainerResource> vault = builder.AddContainer("vault", "hashicorp/vault")
    .WithAnnotation(new ContainerImageAnnotation { Tag = "latest" })
    .WithHttpEndpoint(port: 8200, targetPort: 8200)
    .WithEnvironment("VAULT_ADDR", "http://0.0.0.0:8200")
    .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", vaultToken)
    .WithBindMount("./resource/hashicorp/entrypoint.sh", "/usr/local/bin/entrypoint.sh")
    .WithEntrypoint("/bin/sh -c chmod +x /usr/local/bin/entrypoint.sh && /usr/local/bin/entrypoint.sh")
    .WithAnnotation(new ExecutableArgsCallbackAnnotation(args =>
    {
        args.Add("--cap-add");
        args.Add("IPC_LOCK");
        return args;
    }));
*/

await builder.Build().RunAsync();