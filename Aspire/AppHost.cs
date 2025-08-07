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
IResourceBuilder<SqlServerServerResource> devServer = builder.AddSqlServer("DevServer", devServerPassword, 1433)
    .WithLifetime(ContainerLifetime.Session)
    .WithImage("mssql/server", "2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("TZ", "Europe/London")
    .WithDataVolume("mssql_data");


IResourceBuilder<ParameterResource> keycloakUsername = builder.AddParameter("KeycloakUsername", secret: true);
IResourceBuilder<ParameterResource> keycloakPassword = builder.AddParameter("KeycloakPassword", secret: true);

builder.AddKeycloak("tms-keycloak", port: 8443, keycloakUsername, keycloakPassword)
    .WithLifetime(ContainerLifetime.Session)
    .WithImageTag("latest")
    .WaitFor(devServer)
    .WithEnvironment("KC_DB", "mssql")
    .WithEnvironment("KC_DB_URL", $"jdbc:sqlserver://{devServer.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{devServer.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)};databaseName=keycloak;trustServerCertificate=true")
    .WithEnvironment("KC_DB_SCHEMA", "dbo")
    .WithEnvironment("KC_PROXY", "edge")
    .WithEnvironment("KC_DB_USERNAME", "sa")
    .WithEnvironment("KC_DB_PASSWORD", devServerPassword)
    .WithEnvironment("KC_HOSTNAME", "localhost")
    .WithEnvironment("KC_HOSTNAME_STRICT_HTTPS", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "true")
    .WithEnvironment("KC_HTTP_ENABLED", "false")
    .WithEnvironment("KC_HTTPS_PORT", "8443")
    .WithEnvironment("KC_DB_POOL_INITIAL_SIZE", "5")
    .WithEnvironment("KC_DB_POOL_MIN_SIZE", "5")
    .WithEnvironment("KC_DB_POOL_MAX_SIZE", "100")
    .WithEnvironment("KC_PROXY_ADDRESS_FORWARDING", "false")
    .WithEnvironment("KC_CACHE", "local")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_METRICS_ENABLED", "true")
    .WithEnvironment("KC_LOG_LEVEL", "INFO")
    .WithEnvironment("KC_LOG_CONSOLE_OUTPUT", "default")
    .WithEnvironment("KC_TRANSACTION_XA_ENABLED", "false")
    .WithEnvironment("KC_HTTPS_CERTIFICATE_FILE", "/opt/keycloak/certs/localhost.pem")
    .WithEnvironment("KC_HTTPS_CERTIFICATE_KEY_FILE", " /opt/keycloak/certs/localhost-key.pem")
    .WithBindMount("./resources/keycloak/certs", "/opt/keycloak/certs", true)
    .WithBindMount("./resources/keycloak/maui_realm.json", "/opt/keycloak/data/import/maui_realm.json", true)
    .WithArgs("start-dev --import-realm");

await builder.Build().RunAsync();