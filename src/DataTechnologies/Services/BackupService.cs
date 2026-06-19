using System.Globalization;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Neo4j.Driver;
using Npgsql;
using StackExchange.Redis;

namespace DataTechnologies.Services
{
    public class BackupResult
    {
        public bool Success => Errors.Count == 0;
        public string BackupPath { get; init; } = "";
        public IReadOnlyList<string> Errors { get; init; } = new List<string>();
        public IReadOnlyList<string> Log { get; init; } = new List<string>();
    }

    public class BackupService
    {
        private const string PostgresConnectionString =
            "Host=145.24.237.123;Port=8432;Username=team5;Password=pRnj7a4Gh;Database=team5;";
        private const string RedisConnectionString =
            "145.24.237.123:8379,password=pRnj7a4Gh,user=team5";

        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public string OutputBasePath { get; }
        public int RetentionDays { get; }

        public BackupService(string outputBasePath = "backups", int retentionDays = 30)
        {
            OutputBasePath = outputBasePath;
            RetentionDays = retentionDays;
        }

        public async Task<BackupResult> RunFullBackupAsync()
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
            var backupDir = Path.Combine(OutputBasePath, $"backup_{timestamp}");
            Directory.CreateDirectory(backupDir);

            var errors = new List<string>();
            var log = new List<string>();

            await BackupPostgreSQLAsync(backupDir, errors, log);
            await BackupMongoDBAsync(backupDir, errors, log);
            await BackupRedisAsync(backupDir, errors, log);
            await BackupNeo4jAsync(backupDir, errors, log);

            CleanupOldBackups(log);

            await AppendToLogFileAsync(backupDir, log, errors);

            return new BackupResult { BackupPath = backupDir, Errors = errors, Log = log };
        }

        private static async Task BackupPostgreSQLAsync(string dir, List<string> errors, List<string> log)
        {
            try
            {
                await using var connection = new NpgsqlConnection(PostgresConnectionString);
                await connection.OpenAsync();

                var tables = new List<string>();
                await using (var cmd = new NpgsqlCommand(
                    "SELECT tablename FROM pg_tables WHERE schemaname = 'public'", connection))
                await using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                        tables.Add(reader.GetString(0));

                var allData = new Dictionary<string, List<Dictionary<string, object?>>>();
                foreach (var table in tables)
                {
                    var rows = new List<Dictionary<string, object?>>();
#pragma warning disable CA2100
                    await using var cmd = new NpgsqlCommand($"SELECT * FROM \"{table}\"", connection);
#pragma warning restore CA2100
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object?>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : ConvertForJson(reader.GetValue(i));
                        rows.Add(row);
                    }
                    allData[table] = rows;
                }

                var json = JsonSerializer.Serialize(allData, JsonOptions);
                await File.WriteAllTextAsync(Path.Combine(dir, "postgresql.json"), json);
                log.Add($"PostgreSQL: {tables.Count} tables backed up");
            }
            catch (Exception ex)
            {
                errors.Add($"PostgreSQL backup failed: {ex.Message}");
            }
        }

        private static async Task BackupMongoDBAsync(string dir, List<string> errors, List<string> log)
        {
            try
            {
                var mongoUri = Environment.GetEnvironmentVariable("MONGODB_URI")
                    ?? throw new InvalidOperationException("MONGODB_URI not set");
                var mongoUrl = new MongoUrl(mongoUri);
                var client = new MongoClient(mongoUri);
                var db = client.GetDatabase(string.IsNullOrWhiteSpace(mongoUrl.DatabaseName) ? "team5" : mongoUrl.DatabaseName);

                var allData = new BsonDocument();
                var collectionNames = await (await db.ListCollectionNamesAsync()).ToListAsync();
                foreach (var name in collectionNames)
                {
                    var docs = await db.GetCollection<BsonDocument>(name)
                        .Find(FilterDefinition<BsonDocument>.Empty)
                        .ToListAsync();
                    allData[name] = new BsonArray(docs);
                }

                await File.WriteAllTextAsync(Path.Combine(dir, "mongodb.json"),
                    allData.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true }));
                log.Add($"MongoDB: {collectionNames.Count} collections backed up");
            }
            catch (Exception ex)
            {
                errors.Add($"MongoDB backup failed: {ex.Message}");
            }
        }

        private static async Task BackupRedisAsync(string dir, List<string> errors, List<string> log)
        {
            try
            {
                var redis = await ConnectionMultiplexer.ConnectAsync(RedisConnectionString);
                var db = redis.GetDatabase();
                var server = redis.GetServer(redis.GetEndPoints()[0]);

                var allData = new Dictionary<string, object?>();
                await foreach (var key in server.KeysAsync(database: db.Database))
                {
                    var type = await db.KeyTypeAsync(key);
                    allData[key.ToString()] = type switch
                    {
                        RedisType.String => (object?)(string?)(await db.StringGetAsync(key)),
                        RedisType.Hash => (await db.HashGetAllAsync(key))
                            .ToDictionary(e => e.Name.ToString(), e => (object?)(string?)e.Value),
                        RedisType.List => (await db.ListRangeAsync(key))
                            .Select(v => (string?)v).ToArray(),
                        RedisType.Set => (await db.SetMembersAsync(key))
                            .Select(v => (string?)v).ToArray(),
                        RedisType.SortedSet => (await db.SortedSetRangeByRankWithScoresAsync(key))
                            .Select(e => new { Member = (string?)e.Element, e.Score })
                            .ToArray<object>(),
                        _ => null
                    };
                }

                var json = JsonSerializer.Serialize(allData, JsonOptions);
                await File.WriteAllTextAsync(Path.Combine(dir, "redis.json"), json);
                log.Add($"Redis: {allData.Count} keys backed up");
            }
            catch (Exception ex)
            {
                errors.Add($"Redis backup failed: {ex.Message}");
            }
        }

        private static async Task BackupNeo4jAsync(string dir, List<string> errors, List<string> log)
        {
            try
            {
                var neo4jUri = Environment.GetEnvironmentVariable("NEO4J_URI")
                    ?? throw new InvalidOperationException("NEO4J_URI not set");
                var neo4jUser = Environment.GetEnvironmentVariable("NEO4J_USER") ?? "neo4j";
                var neo4jPassword = Environment.GetEnvironmentVariable("NEO4J_PASSWORD")
                    ?? throw new InvalidOperationException("NEO4J_PASSWORD not set");

                await using var driver = GraphDatabase.Driver(new Uri(neo4jUri), AuthTokens.Basic(neo4jUser, neo4jPassword));
                await driver.VerifyConnectivityAsync();
                await using var session = driver.AsyncSession();

                var nodes = new List<Dictionary<string, object?>>();
                var nodesResult = await session.RunAsync("MATCH (n) RETURN n, labels(n) AS labels, id(n) AS id");
                await nodesResult.ForEachAsync(record =>
                {
                    var node = record["n"].As<INode>();
                    var nodeData = node.Properties
                        .ToDictionary(kv => kv.Key, kv => ConvertForJson(kv.Value));
                    nodeData["_id"] = record["id"].As<long>();
                    nodeData["_labels"] = record["labels"].As<List<string>>();
                    nodes.Add(nodeData);
                });

                var relationships = new List<Dictionary<string, object?>>();
                var relsResult = await session.RunAsync(
                    "MATCH ()-[r]->() RETURN r, type(r) AS type, id(startNode(r)) AS startId, id(endNode(r)) AS endId, id(r) AS id");
                await relsResult.ForEachAsync(record =>
                {
                    var rel = record["r"].As<IRelationship>();
                    var relData = rel.Properties
                        .ToDictionary(kv => kv.Key, kv => ConvertForJson(kv.Value));
                    relData["_id"] = record["id"].As<long>();
                    relData["_type"] = (object?)record["type"].As<string>();
                    relData["_startId"] = record["startId"].As<long>();
                    relData["_endId"] = record["endId"].As<long>();
                    relationships.Add(relData);
                });

                var allData = new { nodes, relationships };
                var json = JsonSerializer.Serialize(allData, JsonOptions);
                await File.WriteAllTextAsync(Path.Combine(dir, "neo4j.json"), json);
                log.Add($"Neo4j: {nodes.Count} nodes, {relationships.Count} relationships backed up");
            }
            catch (Exception ex)
            {
                errors.Add($"Neo4j backup failed: {ex.Message}");
            }
        }

        private void CleanupOldBackups(List<string> log)
        {
            if (!Directory.Exists(OutputBasePath)) return;

            var cutoff = DateTime.Now.AddDays(-RetentionDays);
            int removed = 0;
            foreach (var dir in Directory.GetDirectories(OutputBasePath, "backup_*"))
            {
                if (new DirectoryInfo(dir).CreationTime < cutoff)
                {
                    Directory.Delete(dir, recursive: true);
                    removed++;
                }
            }
            if (removed > 0)
                log.Add($"Cleanup: {removed} backup(s) older than {RetentionDays} days removed");
        }

        private async Task AppendToLogFileAsync(string backupDir, List<string> log, List<string> errors)
        {
            var logPath = Path.Combine(OutputBasePath, "backup.log");
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var lines = new List<string> { $"[{timestamp}] Backup: {backupDir}" };
            lines.AddRange(log.Select(l => $"  {l}"));
            lines.AddRange(errors.Select(e => $"  ERROR: {e}"));
            lines.Add("");
            await File.AppendAllLinesAsync(logPath, lines);
        }

        private static object? ConvertForJson(object? value) => value switch
        {
            null => null,
            string s => s,
            long l => l,
            int i => i,
            short s => (int)s,
            double d => d,
            float f => f,
            decimal d => (double)d,
            bool b => b,
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
            Guid g => g.ToString(),
            byte[] bytes => Convert.ToBase64String(bytes),
            System.Collections.IEnumerable list => list.Cast<object?>().Select(ConvertForJson).ToList(),
            _ => value.ToString()
        };
    }
}
