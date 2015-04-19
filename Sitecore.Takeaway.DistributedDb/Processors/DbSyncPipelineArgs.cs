using Microsoft.Synchronization;
using Sitecore.Pipelines;
using Sitecore.Takeaway.DistributedDb.Configuration;
using System;
using System.Collections.Generic;

namespace Sitecore.Takeaway.DistributedDb.Processors
{
    [Serializable]
    public class DbSyncPipelineArgs : PipelineArgs
    {
        public Dictionary<string, SyncOperationStatistics> Statistics { get; set; }

        public List<SyncDatabase> Databases { get; set; }

        public string Server { get; set; }
    }
}