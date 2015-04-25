using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace Sitecore.Takeaway.DistributedDb.Core
{
    public class DbSyncClient : DbSyncBase
    {
        public DbSyncClient(SyncConfiguration config)
            : base(config)
        {
        }

        public override void Provision()
        {
            Log.Info("[DistributedDb] Provision Client Start", this);

            foreach (var db in base.Databases)
            {
                var clientConn = new SqlConnection(db.ClientConnectionString);
                var serverConn = new SqlConnection(db.ServerConnectionString);
                var scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(db.Scope, serverConn);
                base.Provision(db, clientConn, scopeDesc);
            }

            Log.Info("[DistributedDb] Provision Client End", this);
        }

        public override void ProvisionTriggerAndProcedureUpdates()
        {
            Log.Info("[DistributedDb] ProvisionTriggerAndProcedureUpdates Client Start", this);

            foreach (var db in base.Databases)
            {
                var clientConn = new SqlConnection(db.ClientConnectionString);
                var serverConn = new SqlConnection(db.ServerConnectionString);
                var scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(db.Scope, serverConn);
                base.ProvisionTriggerAndProcedureUpdates(db, clientConn, scopeDesc);
            }

            Log.Info("[DistributedDb] ProvisionTriggerAndProcedureUpdates Client End", this);
        }

        public override void ReorganizeIndexes()
        {
            Log.Info("[DistributedDb] ReorganizeIndexes Client Start", this);

            foreach (var db in base.Databases)
            {
                var clientConn = new SqlConnection(db.ClientConnectionString);
                var serverConn = new SqlConnection(db.ServerConnectionString);
                var scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(db.Scope, serverConn);
                base.ReorganizeIndexes(db.Tables, clientConn, scopeDesc);
            }

            Log.Info("[DistributedDb] ReorganizeIndexes Client End", this);
        }

        public void TruncateClientTables()
        {
            Log.Info("[DistributedDb] TruncateClientTables Client Start", this);

            foreach (var db in base.Databases)
            {
                var clientConn = new SqlConnection(db.ClientConnectionString);
                var serverConn = new SqlConnection(db.ServerConnectionString);
                var scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(db.Scope, serverConn);
                base.TruncateTables(db.Tables, clientConn, scopeDesc);
            }

            Log.Info("[DistributedDb] TruncateClientTables Client End", this);
        }

        public override void Deprovision()
        {
            Log.Info("[DistributedDb] Deprovision Client Start", this);

            foreach (var db in base.Databases)
            {
                var clientConn = new SqlConnection(db.ClientConnectionString);
                var serverConn = new SqlConnection(db.ServerConnectionString);
                var scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(db.Scope, serverConn);
                base.Deprovision(db, clientConn, scopeDesc);
            }

            base.DeprovisionStore(new SqlConnection(base.Configuration.ClientConnectionString));

            Log.Info("[DistributedDb] Deprovision Client End", this);
        }

        public Dictionary<string, SyncOperationStatistics> Synchronize()
        {
            return Synchronize(SyncDirectionOrder.UploadAndDownload);
        }

        public Dictionary<string, SyncOperationStatistics> Synchronize(SyncDirectionOrder direction)
        {
            Log.Info("[DistributedDb] Synchronize Client Start", this);

            var result = new Dictionary<string, SyncOperationStatistics>();

            foreach (var db in base.Databases)
            {
                try
                {
                    Log.Info("[DistributedDb] Synchronize Client [" + db.Scope + "] Start", this);

                    var clientConn = new SqlConnection(db.ClientConnectionString);
                    var serverConn = new SqlConnection(db.ServerConnectionString);

                    var syncOrchestrator = new SyncOrchestrator();

                    var client = new SqlSyncProvider(db.Scope, clientConn);
                    var server = new SqlSyncProvider(db.Scope, serverConn);

                    client.CommandTimeout = 3600;
                    server.CommandTimeout = 3600;

                    syncOrchestrator.LocalProvider = client;
                    syncOrchestrator.RemoteProvider = server;
                    syncOrchestrator.Direction = direction;

                    ((SqlSyncProvider)syncOrchestrator.LocalProvider).ApplyChangeFailed += new EventHandler<DbApplyChangeFailedEventArgs>(SynchronizeClient_ApplyChangeFailed);
                    ((SqlSyncProvider)syncOrchestrator.RemoteProvider).ApplyChangeFailed += new EventHandler<DbApplyChangeFailedEventArgs>(SynchronizeServer_ApplyChangeFailed);
                    ((SqlSyncProvider)syncOrchestrator.LocalProvider).ApplyingChanges += new EventHandler<DbApplyingChangesEventArgs>(SynchronizeClient_ApplyingChanges);
                    ((SqlSyncProvider)syncOrchestrator.RemoteProvider).ApplyingChanges += new EventHandler<DbApplyingChangesEventArgs>(SynchronizeServer_ApplyingChanges);
                    ((SqlSyncProvider)syncOrchestrator.LocalProvider).ChangesApplied += new EventHandler<DbChangesAppliedEventArgs>(SynchronizeClient_ChangesApplied);
                    ((SqlSyncProvider)syncOrchestrator.RemoteProvider).ChangesApplied += new EventHandler<DbChangesAppliedEventArgs>(SynchronizeServer_ChangesApplied);
                    ((SqlSyncProvider)syncOrchestrator.LocalProvider).SelectingChanges += new EventHandler<DbSelectingChangesEventArgs>(SynchronizeClient_SelectingChanges);
                    ((SqlSyncProvider)syncOrchestrator.RemoteProvider).SelectingChanges += new EventHandler<DbSelectingChangesEventArgs>(SynchronizeServer_SelectingChanges);
                    ((SqlSyncProvider)syncOrchestrator.LocalProvider).ChangesSelected += new EventHandler<DbChangesSelectedEventArgs>(SynchronizeClient_ChangesSelected);
                    ((SqlSyncProvider)syncOrchestrator.RemoteProvider).ChangesSelected += new EventHandler<DbChangesSelectedEventArgs>(SynchronizeServer_ChangesSelected);
                    //((SqlSyncProvider)syncOrchestrator.LocalProvider).SyncProgress += new EventHandler<DbSyncProgressEventArgs>(Synchronize_SyncProgress);
                    syncOrchestrator.SessionProgress += new EventHandler<SyncStagedProgressEventArgs>(Synchronize_ProgressChanged);

                    var syncStats = syncOrchestrator.Synchronize();

                    Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] SyncStartTime: " + syncStats.SyncStartTime.ToString("hh:mm:ss.fff"), this);
                    if (syncStats.UploadChangesTotal > 0)
                        Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] UploadChangesTotal: " + syncStats.UploadChangesTotal, this);
                    if (syncStats.DownloadChangesTotal > 0)
                        Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] DownloadChangesTotal: " + syncStats.DownloadChangesTotal, this);
                    if (syncStats.UploadChangesApplied > 0)
                        Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] UploadChangesApplied: " + syncStats.UploadChangesApplied, this);
                    if (syncStats.DownloadChangesApplied > 0)
                        Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] DownloadChangesApplied: " + syncStats.DownloadChangesApplied, this);
                    if (syncStats.UploadChangesFailed > 0)
                        Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] UploadChangesFailed: " + syncStats.UploadChangesFailed, this);
                    if (syncStats.DownloadChangesFailed > 0)
                        Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] DownloadChangesFailed: " + syncStats.DownloadChangesFailed, this);
                    Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] SyncEndTime: " + syncStats.SyncEndTime.ToString("hh:mm:ss.fff"), this);

                    result.Add(db.Scope, syncStats);

                    Log.Info("[DistributedDb] Synchronize Client [" + db.Scope + "] End", this);
                }
                catch (Exception ex)
                {
                    Log.Error("[DistributedDb] Synchronize Client [" + db.Scope + "] Error", ex, this);
                }
            }

            Log.Info("[DistributedDb] Synchronize Client Scope End", this);

            return result;
        }

        public void SynchronizeClient_ApplyChangeFailed(object sender, DbApplyChangeFailedEventArgs e)
        {
            if (e.Conflict != null)
                Log.Error("[DistributedDb] Synchronize Client ApplyChangeFailed [" + e.Conflict.RemoteChange.TableName + "] Conflict", new Exception(e.Conflict.Type.ToString()), this);

            if (e.Error != null)
                Log.Error("[DistributedDb] Synchronize Client ApplyChangeFailed [" + e.Conflict.RemoteChange.TableName + "] Error", e.Error, this);
        }

        public void SynchronizeServer_ApplyChangeFailed(object sender, DbApplyChangeFailedEventArgs e)
        {
            if (e.Error != null)
                Log.Error("[DistributedDb] Synchronize Server ApplyChangeFailed Error", e.Error, this);
        }

        public void Synchronize_ProgressChanged(object sender, SyncStagedProgressEventArgs args)
        {
            if (args.Stage != SessionProgressStage.ChangeDetection)
            {
                Log.Info("[DistributedDb] Synchronize Progress Changed: Provider - " + args.ReportingProvider.ToString(), this);
                Log.Info("[DistributedDb] Synchronize Progress Changed: Stage - " + args.Stage.ToString(), this);
                Log.Info("[DistributedDb] Synchronize Progress Changed: Work - " + args.CompletedWork + " of " + args.TotalWork, this);
            }
        }

        public void SynchronizeClient_ApplyingChanges(object sender, DbApplyingChangesEventArgs args)
        {
            LogChanges(args.Context.ScopeProgress.TablesProgress, "Client Applying Changes");
        }

        public void SynchronizeServer_ApplyingChanges(object sender, DbApplyingChangesEventArgs args)
        {
            LogChanges(args.Context.ScopeProgress.TablesProgress, "Server Applying Changes");
        }

        public void LogChanges(Collection<DbSyncTableProgress> tables, string source)
        {
            foreach (var progress in tables)
            {
                if (progress.TotalChanges > 0)
                {
                    Log.Info("[DistributedDb] Synchronize " + source + " [" + progress.TableName + "] TotalChanges : " + progress.TotalChanges, this);
                    if (progress.Inserts > 0) 
                        Log.Info("[DistributedDb] Synchronize " + source + " [" + progress.TableName + "] Inserts : " + progress.Inserts, this);
                    if (progress.Updates > 0) 
                        Log.Info("[DistributedDb] Synchronize " + source + " [" + progress.TableName + "] Updates : " + progress.Updates, this);
                    if (progress.Deletes > 0) 
                        Log.Info("[DistributedDb] Synchronize " + source + " [" + progress.TableName + "] Deletes : " + progress.Deletes, this);
                    if (progress.ChangesApplied > 0)
                        Log.Info("[DistributedDb] Synchronize " + source + " [" + progress.TableName + "] ChangesApplied : " + progress.ChangesApplied, this);
                    if (progress.ChangesFailed > 0)
                        Log.Info("[DistributedDb] Synchronize " + source + " [" + progress.TableName + "] ChangesFailed : " + progress.ChangesFailed, this);
                    if (progress.ChangesPending > 0)
                        Log.Info("[DistributedDb] Synchronize " + source + " [" + progress.TableName + "] ChangesPending : " + progress.ChangesPending, this);
                    
                }
            }
        }

        public void SynchronizeClient_ChangesApplied(object sender, DbChangesAppliedEventArgs args)
        {
            LogChanges(args.Context.ScopeProgress.TablesProgress, "Client Changes Applied");
        }

        public void SynchronizeServer_ChangesApplied(object sender, DbChangesAppliedEventArgs args)
        {
            LogChanges(args.Context.ScopeProgress.TablesProgress, "Server Changes Applied");
        }

        public void SynchronizeClient_SelectingChanges(object sender, DbSelectingChangesEventArgs args)
        {
            Log.Info("[DistributedDb] Synchronize Client Selecting Changes", this);
        }

        public void SynchronizeServer_SelectingChanges(object sender, DbSelectingChangesEventArgs args)
        {
            Log.Info("[DistributedDb] Synchronize Server Selecting Changes", this);
        }

        public void SynchronizeClient_ChangesSelected(object sender, DbChangesSelectedEventArgs args)
        {
            LogChanges(args.Context.ScopeProgress.TablesProgress, "Client Changes Selected");
        }

        public void SynchronizeServer_ChangesSelected(object sender, DbChangesSelectedEventArgs args)
        {
            LogChanges(args.Context.ScopeProgress.TablesProgress, "Server Changes Selected");
        }

    }
}