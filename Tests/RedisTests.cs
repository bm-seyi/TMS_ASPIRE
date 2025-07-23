using StackExchange.Redis;


namespace Tests
{
    [TestClass]
    public class RedisTests
    {
        [TestMethod]
        public async Task RedisResourceIsHealthyAndConnectable()
        {
            var (connection, app) = await RedisConnection();
            await using (app)
            {
                IDatabase db = connection.GetDatabase();
                TimeSpan ping = await db.PingAsync();

                Assert.IsTrue(ping.TotalMilliseconds < 100, "Redis ping should be less than 100ms");
            }
        }

        [TestMethod]
        public async Task RedisResourceCanSetAndGet()
        {
            var (connection, app) = await RedisConnection();

            await using (app)
            {
                IDatabase db = connection.GetDatabase();

                string key = "test-key";
                string value = "test-value";

                bool setResult = await db.StringSetAsync(key, value);
                string? retrievedValue = await db.StringGetAsync(key);

                Assert.IsTrue(setResult, "Failed to set value in Redis");
                Assert.AreEqual(value, retrievedValue, "Retrieved value does not match the set value");
            }
        }

        [TestMethod]
        public async Task RedisResourceMultipleConnections()
        {
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.TMS_ASPIRE>();

            await using var app = await builder.BuildAsync();
            await app.StartAsync();

            // Act
            string connectionString = await app.GetConnectionStringAsync("redis-signalr") ?? throw new ArgumentNullException(nameof(connectionString));

            for (int i = 0; i < 30; i++)
            {
                ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
                Assert.IsNotNull(connection, $"Connection {i + 1} should not be null");
                Assert.IsTrue(connection.IsConnected, $"Connection {i + 1} should be connected");
            }
        }

        [TestMethod]
        public async Task RedisResourceCanSetAndGetWithMultipleConnections()
        {
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.TMS_ASPIRE>();

            await using var app = await builder.BuildAsync();
            await app.StartAsync();

            // Act
            string connectionString = await app.GetConnectionStringAsync("redis-signalr") ?? throw new ArgumentNullException(nameof(connectionString));

            for (int i = 0; i < 30; i++)
            {
                ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
                IDatabase db = connection.GetDatabase();

                string key = $"test-key-{i}";
                string value = $"test-value-{i}";

                bool setResult = await db.StringSetAsync(key, value);
                string? retrievedValue = await db.StringGetAsync(key);

                Assert.IsTrue(setResult, $"Failed to set value in Redis for connection {i + 1}");
                Assert.AreEqual(value, retrievedValue, $"Retrieved value does not match the set value for connection {i + 1}");
            }
        }

        [TestMethod]
        public async Task RedisResourceNotRetrieveExpiredKeys()
        {

            var (connection, app) = await RedisConnection();
            await using (app)
            {
                IDatabase db = connection.GetDatabase();

                string key = "test-key";
                string value = "test-value";

                bool setResult = await db.StringSetAsync(key, value, TimeSpan.FromSeconds(1));
                await Task.Delay(2000);
                string? retrievedValue = await db.StringGetAsync(key);
                Assert.IsTrue(setResult, "Failed to set value in Redis with expiration");
                Assert.IsNull(retrievedValue, "Retrieved value should be null for expired key");
                Assert.IsFalse(await db.KeyExistsAsync(key), "Key should not exist after expiration");
            }
        }

        [TestMethod]
        public async Task RedisResourceConcurrentAccess()
        {
            var (redis, app) = await RedisConnection();
            await using (app)
            {
                var db = redis.GetDatabase();

                string testKey = "concurrent:test";
                await db.KeyDeleteAsync(testKey);

                int threadCount = 20;
                int incrementsPerThread = 100;
                var tasks = new List<Task>();

                for (int i = 0; i < threadCount; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        for (int j = 0; j < incrementsPerThread; j++)
                        {
                            // INCR is atomic in Redis
                            await db.StringIncrementAsync(testKey);
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                // Verify final value
                var finalValue = await db.StringGetAsync(testKey);
                int expected = threadCount * incrementsPerThread;

                Assert.AreEqual(expected.ToString(), finalValue.ToString(), "Concurrent INCR failed to be atomic");
            }

        }

        [TestMethod]
        public async Task RedisResourceInvalidConnectionString()
        {

            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.TMS_ASPIRE>();

            await using var app = await builder.BuildAsync();
            await app.StartAsync();

            string invalidConnectionString = "invalid-connection-string";

            try
            {
                ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(invalidConnectionString);
                Assert.Fail("Expected exception for invalid connection string");
            }
            catch (RedisConnectionException ex)
            {
                Assert.IsTrue(ex.Message.Contains("It was not possible to connect to the redis server(s)"), "Expected connection error for invalid connection string");
            }
        }


        private async Task<(ConnectionMultiplexer connection, IAsyncDisposable app)> RedisConnection()
        {
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.TMS_ASPIRE>();

            var app = await builder.BuildAsync();
            await app.StartAsync();

            string connectionString = await app.GetConnectionStringAsync("redis-signalr")
                ?? throw new ArgumentNullException(nameof(connectionString));

            ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(connectionString);

            return (connection, app);
        }
    }
}


