using Microsoft.Synchronization.Data;
using Sitecore.Diagnostics;
using System.Data.SqlClient;

namespace Sitecore.Takeaway.DistributedDb.Core
{
    public class DbSyncServer : DbSyncBase
    {
        public DbSyncServer(SyncConfiguration config)
            : base(config)
        {
        }

        public override void Provision()
        {
            Log.Info("[DistributedDb] Provision Server Start", this);

            foreach (var db in base.Databases)
            {
                var serverConn = new SqlConnection(db.ServerConnectionString);
                var scopeDesc = new DbSyncScopeDescription(db.Scope);
                this.GetDescriptionForTables(db.Tables, serverConn, ref scopeDesc);
                base.Provision(db, serverConn, scopeDesc);
            }

            Log.Info("[DistributedDb] Provision Server End", this);
        }

        public override void ProvisionTriggerAndProcedureUpdates()
        {
            Log.Info("[DistributedDb] ProvisionTriggerAndProcedureUpdates Server Start", this);

            foreach (var db in base.Databases)
            {
                var serverConn = new SqlConnection(db.ServerConnectionString);
                var scopeDesc = new DbSyncScopeDescription(db.Scope);
                this.GetDescriptionForTables(db.Tables, serverConn, ref scopeDesc);
                base.ProvisionTriggerAndProcedureUpdates(db, serverConn, scopeDesc);
            }

            Log.Info("[DistributedDb] ProvisionTriggerAndProcedureUpdates Server End", this);
        }

        public override void ReorganizeIndexes()
        {
            Log.Info("[DistributedDb] ReorganizeIndexes Server Start", this);

            foreach (var db in base.Databases)
            {
                var serverConn = new SqlConnection(db.ServerConnectionString);
                var scopeDesc = new DbSyncScopeDescription(db.Scope);
                this.GetDescriptionForTables(db.Tables, serverConn, ref scopeDesc);
                base.ReorganizeIndexes(db.Tables, serverConn, scopeDesc);
            }

            Log.Info("[DistributedDb] ReorganizeIndexes Server End", this);
        }

        public override void Deprovision()
        {
            Log.Info("[DistributedDb] Deprovision Server Start", this);

            foreach (var db in base.Databases)
            {
                var serverConn = new SqlConnection(db.ServerConnectionString);
                var scopeDesc = new DbSyncScopeDescription(db.Scope);
                base.Deprovision(db, serverConn, scopeDesc);
            }
            base.DeprovisionStore(new SqlConnection(base.Configuration.ServerConnectionString));

            Log.Info("[DistributedDb] Deprovision Server End", this);
        }
    }
}