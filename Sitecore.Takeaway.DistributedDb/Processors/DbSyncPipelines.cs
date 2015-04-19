using Sitecore.Diagnostics;
using Sitecore.Takeaway.DistributedDb.Core;

namespace Sitecore.Takeaway.DistributedDb.Processors
{
    public class ProvisionServer
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline ProvisionServer Start - " + args.Server, this);
            var syncManager = new DbSyncManager();
            syncManager.Server.Provision();

            args.Databases = syncManager.Server.Databases;

            Log.Info("[DistributedDb] Pipeline ProvisionServer End - " + args.Server, this);
        }
    }

    public class ProvisionTriggerAndProcedureUpdatesServer
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline ProvisionTriggerAndProcedureUpdates Start - " + args.Server, this);

            var syncManager = new DbSyncManager();
            syncManager.Server.ProvisionTriggerAndProcedureUpdates();

            args.Databases = syncManager.Server.Databases;

            Log.Info("[DistributedDb] Pipeline ProvisionTriggerAndProcedureUpdates End - " + args.Server, this);
        }
    }

    public class ReorganizeIndexesServer
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline ReorganizeIndexes Start - " + args.Server, this);
            var syncManager = new DbSyncManager();
            syncManager.Server.ReorganizeIndexes();
            args.Databases = syncManager.Server.Databases;

            Log.Info("[DistributedDb] Pipeline ReorganizeIndexes End - " + args.Server, this);
        }
    }

    public class DeprovisionServer
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline DeprovisionServer Start - " + args.Server, this);
            var syncManager = new DbSyncManager();
            syncManager.Server.Deprovision();
            args.Databases = syncManager.Server.Databases;
            Log.Info("[DistributedDb] Pipeline DeprovisionServer End - " + args.Server, this);
        }
    }

    public class ProvisionClient
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline ProvisionClient Start - " + args.Server, this);
            var syncManager = new DbSyncManager();
            syncManager.Client.Provision();
            args.Databases = syncManager.Client.Databases;
            Log.Info("[DistributedDb] Pipeline ProvisionClient End - " + args.Server, this);
        }
    }

    public class ProvisionTriggerAndProcedureUpdatesClient
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline ProvisionTriggerAndProcedureUpdatesClient Start - " + args.Server, this);
            var syncManager = new DbSyncManager();
            syncManager.Client.ProvisionTriggerAndProcedureUpdates();
            args.Databases = syncManager.Client.Databases;
            Log.Info("[DistributedDb] Pipeline ProvisionTriggerAndProcedureUpdatesClient End - " + args.Server, this);
        }
    }

    public class ReorganizeIndexesClient
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline ReorganizeIndexesClient Start - " + args.Server, this);
            var syncManager = new DbSyncManager();
            syncManager.Client.ReorganizeIndexes();
            args.Databases = syncManager.Client.Databases;
            Log.Info("[DistributedDb] Pipeline ReorganizeIndexesClient End - " + args.Server, this);
        }
    }

    public class DeprovisionClient
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline DeprovisionClient Start - " + args.Server, this);
            var syncManager = new DbSyncManager();
            syncManager.Client.Deprovision();
            args.Databases = syncManager.Client.Databases;
            Log.Info("[DistributedDb] Pipeline DeprovisionClient End - " + args.Server, this);
        }
    }

    public class Synchronize
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline Synchronize Start - " + args.Server, this);
            var syncManager = new DbSyncManager();
            args.Statistics = syncManager.Client.Synchronize();
            args.Databases = syncManager.Client.Databases;
            Log.Info("[DistributedDb] Pipeline Synchronize End - " + args.Server, this);
        }
    }

    public class TruncateClientTables
    {
        public void Process(DbSyncPipelineArgs args)
        {
            Log.Info("[DistributedDb] Pipeline TruncateClientTables Start - " + args.Server, this);
            var syncManager = new DbSyncManager();
            syncManager.Client.TruncateClientTables();
            args.Databases = syncManager.Client.Databases;
            Log.Info("[DistributedDb] Pipeline TruncateClientTables End - " + args.Server, this);
        }
    }
}