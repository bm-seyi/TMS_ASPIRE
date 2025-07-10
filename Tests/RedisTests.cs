using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Microsoft.Extensions.Hosting;
using Aspire.Hosting.ApplicationModel;


namespace Tests;

[TestClass]
public class RedisTests
{
    [TestMethod]
    public async Task RedisResourceIsHealthyAndConnectable()
    {
        // Arrange
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.TMS_ASPIRE>();

        await using var app = await builder.BuildAsync();
        await app.StartAsync();

        // Act
        string connectionString = await app.GetConnectionStringAsync("redis-signalr") ?? throw new ArgumentNullException(nameof(connectionString));
        ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(connectionString);

        IDatabase db = connection.GetDatabase();
        TimeSpan ping = await db.PingAsync();
        
        Assert.IsTrue(ping.TotalMilliseconds < 100, "Redis ping should be less than 100ms");
    }
}
