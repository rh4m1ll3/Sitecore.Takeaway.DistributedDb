using Sitecore.Configuration;

namespace Sitecore.Takeaway.DistributedDb.Core
{
    public class DbSyncManager
    {
        public DbSyncManager()
        {
            var config = Factory.CreateObject("distributedDb/syncConfiguration", false) as SyncConfiguration;
            Initialize(config);
        }

        public DbSyncManager(SyncConfiguration config)
        {
            Initialize(config);
        }

        private void Initialize(SyncConfiguration config)
        {
            _server = new DbSyncServer(config);
            _client = new DbSyncClient(config);
        }

        private DbSyncServer _server;

        public DbSyncServer Server
        {
            get { return _server; }
        }

        private DbSyncClient _client;

        public DbSyncClient Client
        {
            get { return _client; }
        }
    }
}