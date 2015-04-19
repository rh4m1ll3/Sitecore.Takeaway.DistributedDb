using System.Collections.Generic;

namespace Sitecore.Takeaway.DistributedDb.Configuration
{
    public class SyncTable
    {
        public string Name { get; set; }

        public SyncDatabase Database { get; set; }

        public List<string> PrimaryKeys { get; set; }

        public List<string> PrimaryKeysToRemove { get; set; }
    }
}