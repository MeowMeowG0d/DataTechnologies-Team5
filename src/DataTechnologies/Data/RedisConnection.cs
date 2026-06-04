using StackExchange.Redis;

namespace DataTechnologies.Data
{
    public class RedisConnection
    {
        private readonly string _connectionString;
        private readonly ConnectionMultiplexer _redis;

        public RedisConnection()
        {
            _connectionString = "145.24.237.123:8379,password=pRnj7a4Gh,user=team5"; //should be env lol
            _redis = ConnectionMultiplexer.Connect(_connectionString);
        }

        public IDatabase GetDatabase()
        {
            return _redis.GetDatabase();
        }
    }
}