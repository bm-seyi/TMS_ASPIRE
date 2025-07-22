using Aspire.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    [TestClass]
    public class DevServerTests
    {
        public DistributedApplication app = null!;
        public string sqlPassword = null!;

        [TestInitialize]
        public async Task Setup()
        {
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.TMS_ASPIRE>();

            builder.Configuration.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);

            sqlPassword = builder.Configuration["DevServerPassword"]
                ?? throw new ArgumentNullException(nameof(sqlPassword));

            app = await builder.BuildAsync();
            await app.StartAsync();
        }

        [TestMethod]
        public async Task DevServerResourceIsConnectable()
        {
            string connectionString = await app.GetConnectionStringAsync("DevServer")
                ?? throw new ArgumentNullException(nameof(connectionString));

            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                Password = sqlPassword
            };

            SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
            await connection.OpenAsync();
            Assert.IsTrue(connection.State == System.Data.ConnectionState.Open, "DevServer connection should be open");
            await connection.DisposeAsync();
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