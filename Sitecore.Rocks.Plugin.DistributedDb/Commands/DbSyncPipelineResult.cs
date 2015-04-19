using System.Collections.Generic;

namespace Sitecore.Rocks.Plugin.DistributedDb.Commands
{
    public class DbSyncPipelineResult
    {
        public string Pipeline { get; set; }

        public List<string> Databases { get; set; }
    }
}