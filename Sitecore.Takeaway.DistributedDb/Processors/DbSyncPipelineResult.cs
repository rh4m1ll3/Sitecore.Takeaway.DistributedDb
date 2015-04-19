using System.Collections.Generic;

namespace Sitecore.Takeaway.DistributedDb.Processors
{
    public class DbSyncPipelineResult
    {
        public string Pipeline { get; set; }

        public List<string> Databases { get; set; }
    }
}