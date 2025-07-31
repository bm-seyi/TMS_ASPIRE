using Aspire.Hosting;
using Microsoft.Data.SqlClient;

namespace Tests
{
    [TestClass]
    public class DevServerTests
    {
        private DistributedApplication app = null!;

        [TestInitialize]
        public async Task Setup()
        {
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.TMS_ASPIRE>();

            builder.AddSqlServer(name: "sql", port: 1433)
                .WithLifetime(ContainerLifetime.Session)
                .WithImage("mssql/server", "2022-latest")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("TZ", "Europe/London")
                .WithDataVolume("test")
                .AddDatabase("sqldb");

            app = await builder.BuildAsync();

            await app.StartAsync();
        }

        [TestMethod]
        public async Task DevServerResourceIsConnectable()
        {
            string connectionString = await app.GetConnectionStringAsync("sql")
                ?? throw new InvalidOperationException("Unable to retrieve connection string for SQL Server");

            var stringBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                Encrypt = false,
                TrustServerCertificate = true,
            };

            await Task.Delay(30000);

            SqlConnection connection = new SqlConnection(stringBuilder.ConnectionString);
            await connection.OpenAsync();
            Assert.IsTrue(connection.State == System.Data.ConnectionState.Open, "DevServer connection should be open");
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            if (app != null)
            {
                await app.StopAsync();
                await app.DisposeAsync();
            }
        }

    }
}