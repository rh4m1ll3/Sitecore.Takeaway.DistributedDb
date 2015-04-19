using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
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
                    //((SqlSyncProvider)syncOrchestrator.LocalProvider).ApplyingChanges += new EventHandler<DbApplyingChangesEventArgs>(Synchronize_ApplyingChanges);
                    //((SqlSyncProvider)syncOrchestrator.LocalProvider).SyncProgress += new EventHandler<DbSyncProgressEventArgs>(Synchronize_SyncProgress);
                    syncOrchestrator.SessionProgress += new EventHandler<SyncStagedProgressEventArgs>(Synchronize_ProgressChanged);

                    var syncStats = syncOrchestrator.Synchronize();

                    Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] SyncStartTime: " + syncStats.SyncStartTime.ToString("hh:mm:ss.fff"), this);
                    Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] UploadChangesTotal: " + syncStats.UploadChangesTotal, this);
                    Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] DownloadChangesTotal: " + syncStats.DownloadChangesTotal, this);
                    Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] UploadChangesApplied: " + syncStats.UploadChangesApplied, this);
                    Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] DownloadChangesApplied: " + syncStats.DownloadChangesApplied, this);
                    Log.Info("[DistributedDb] Synchronize Client Statistics [" + db.Scope + "] UploadChangesFailed: " + syncStats.UploadChangesFailed, this);
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

        public void Synchronize_SyncProgress(object sender, DbSyncProgressEventArgs args)
        {
            string message = "[DistributedDb] Synchronize_SyncProgress Progress: ";

            switch (args.Stage)
            {
                case DbSyncStage.ApplyingInserts:
                    message += "[DistributedDb] Synchronize_SyncProgress Applying insert to table: " + args.TableProgress.TableName;
                    message += "[" + args.TableProgress.Inserts.ToString() + "|" + args.TableProgress.Updates.ToString() + "|" + args.TableProgress.Deletes.ToString() + "]";
                    message += "(Applied:" + args.TableProgress.ChangesApplied.ToString() + "/Pending:" + args.TableProgress.ChangesPending.ToString() +
                       "/Failed:" + args.TableProgress.ChangesFailed.ToString() + "/Total:" + args.TableProgress.TotalChanges.ToString() + ")";
                    break;

                case DbSyncStage.ApplyingUpdates:
                    message += "[DistributedDb] Synchronize_SyncProgress Applying update to table: " + args.TableProgress.TableName;
                    message += "[" + args.TableProgress.Inserts.ToString() + "|" + args.TableProgress.Updates.ToString() + "|" + args.TableProgress.Deletes.ToString() + "]";
                    message += "(Applied:" + args.TableProgress.ChangesApplied.ToString() + "/Pending:" + args.TableProgress.ChangesPending.ToString() +
                    "/Failed:" + args.TableProgress.ChangesFailed.ToString() + "/Total:" + args.TableProgress.TotalChanges.ToString() + ")";
                    break;

                case DbSyncStage.ApplyingDeletes:
                    message += "[DistributedDb] Synchronize_SyncProgress Applying delete to table: " + args.TableProgress.TableName;
                    message += "[" + args.TableProgress.Inserts.ToString() + "|" + args.TableProgress.Updates.ToString() + "|" + args.TableProgress.Deletes.ToString() + "]";
                    message += "(Applied:" + args.TableProgress.ChangesApplied.ToString() + "/Pending:" + args.TableProgress.ChangesPending.ToString() +
                    "/Failed:" + args.TableProgress.ChangesFailed.ToString() + "/Total:" + args.TableProgress.TotalChanges.ToString() + ")";
                    break;

                case DbSyncStage.SelectingChanges:
                    message += "[DistributedDb] Synchronize_SyncProgress Enumerating changes for table: " + args.TableProgress.TableName;
                    message += "[" + args.TableProgress.Inserts.ToString() + "|" + args.TableProgress.Updates.ToString() + "|" + args.TableProgress.Deletes.ToString() + "]";
                    break;

                default:
                    DbSyncStage stage = args.Stage;

                    break;
            }

            Log.Info(message, this);
        }

        public void Synchronize_ProgressChanged(object sender, SyncStagedProgressEventArgs args)
        {
            if (args.Stage != SessionProgressStage.ChangeDetection)
            {
                Log.Info("[DistributedDb] Synchronize_ProgressChanged Progress Changed: provider - " + args.ReportingProvider.ToString(), this);
                Log.Info("[DistributedDb] Synchronize_ProgressChanged stage - " + args.Stage.ToString(), this);
                Log.Info("[DistributedDb] Synchronize_ProgressChanged work - " + args.CompletedWork + " of " + args.TotalWork, this);
            }
        }

        public void Synchronize_ApplyingChanges(object sender, DbApplyingChangesEventArgs args)
        {
            Log.Info("[DistributedDb] Synchronize_ApplyingChanges Sync Stage: Applying Changes", this);
        }

        public void Synchronize_ProgressChangedx(object sender, DbSyncSessionProgressEventArgs args)
        {
            string message = "";
            var a = args.DbSyncStage;

            try
            {
                DbSyncSessionProgressEventArgs sessionProgress = (DbSyncSessionProgressEventArgs)args;
                DbSyncScopeProgress progress = sessionProgress.GroupProgress;
                switch (sessionProgress.DbSyncStage)
                {
                    case DbSyncStage.SelectingChanges:
                        message += "[DistributedDb] Synchronize_ProgressChanged Sync Stage: Selecting Changes";
                        Log.Info(message, this);
                        foreach (DbSyncTableProgress tableProgress in progress.TablesProgress)
                        {
                            message = "[DistributedDb] Synchronize_ProgressChanged Enumerated changes for table: " + tableProgress.TableName;
                            message += "[Inserts:" + tableProgress.Inserts.ToString() + "/Updates :" + tableProgress.Updates.ToString() + "/Deletes :" + tableProgress.Deletes.ToString() + "]";
                            Log.Info(message, this);
                        }
                        break;

                    case DbSyncStage.ApplyingChanges:

                        break;

                    default:
                        break;
                }

                message = "[DistributedDb] Synchronize_ProgressChanged Total Changes : " + progress.TotalChanges.ToString() + "  Inserts :" + progress.TotalInserts.ToString();
                message += "  Updates :" + progress.TotalUpdates.ToString() + "  Deletes :" + progress.TotalDeletes.ToString();
                Log.Info(message, this);
            }
            catch (Exception e)
            {
                Log.Error("[DistributedDb] Synchronize_ProgressChanged ", e, this);
                throw;
            }

            message = "[DistributedDb] Synchronize_ProgressChanged Completed : " + args.CompletedWork + "%";
            Log.Info(message, this);
        }
    }
}