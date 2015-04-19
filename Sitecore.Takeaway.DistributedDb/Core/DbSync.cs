using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Sitecore.Diagnostics;
using Sitecore.Takeaway.DistributedDb.Configuration;
using Sitecore.Takeaway.DistributedDb.Tools;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Sitecore.Takeaway.DistributedDb.Core
{
    public abstract class DbSyncBase
    {
        private SyncConfiguration _config;

        protected SyncConfiguration Configuration
        {
            get
            {
                return _config;
            }
        }

        protected DbSyncBase(SyncConfiguration config)
        {
            Assert.IsNotNull(config, "[DistributedDb] configuration is null");
            _config = config;
        }

        public List<SyncDatabase> Databases
        {
            get
            {
                Assert.IsNotNull(_config, "[DistributedDb] configuration is null");
                return _config.Databases;
            }
        }

        public abstract void Provision();

        internal void Provision(SyncDatabase db, SqlConnection connection, DbSyncScopeDescription scopeDesc)
        {
            var provision = new SqlSyncScopeProvisioning(connection, scopeDesc);

            if (!provision.ScopeExists(scopeDesc.ScopeName))
            {
                try
                {
                    Log.Info("[DistributedDb] Provision Scope [" + scopeDesc.ScopeName + "] Start", this);

                    provision.SetCreateTableDefault(DbSyncCreationOption.Skip);
                    provision.CommandTimeout = 3600;
                    provision.Apply();

                    Log.Info("[DistributedDb] Provision Scope [" + scopeDesc.ScopeName + "] End", this);
                }
                catch (Exception ex)
                {
                    Log.Error("[DistributedDb] Provision Scope [" + scopeDesc.ScopeName + "] Error", ex, this);
                }
            }
            else
                Log.Info("[DistributedDb] Provision Scope [" + scopeDesc.ScopeName + "] Skipped", this);
        }

        public abstract void ProvisionTriggerAndProcedureUpdates();

        internal void ProvisionTriggerAndProcedureUpdates(SyncDatabase db, SqlConnection connection, DbSyncScopeDescription scopeDesc)
        {
            var provision = new SqlSyncScopeProvisioning(connection, scopeDesc);

            if (provision.ScopeExists(scopeDesc.ScopeName))
            {
                try
                {
                    Log.Info("[DistributedDb] Provision Scope Trigger And Procedure Updates [" + scopeDesc.ScopeName + "] Start", this);

                    provision.SetCreateTableDefault(DbSyncCreationOption.Skip);
                    provision.CommandTimeout = 3600;
                    provision.ApplyTriggerAndProcedureUpdates(connection);

                    Log.Info("[DistributedDb] Provision Scope Trigger And Procedure Updates [" + scopeDesc.ScopeName + "] End", this);
                }
                catch (Exception ex)
                {
                    Log.Error("[DistributedDb] Provision Scope Trigger And Procedure Updates [" + scopeDesc.ScopeName + "] Error", ex, this);
                }
            }
            else
                Log.Info("[DistributedDb] Provision Scope Trigger And Procedure Updates [" + scopeDesc.ScopeName + "] Skipped", this);
        }

        public abstract void ReorganizeIndexes();

        internal void ReorganizeIndexes(List<Sitecore.Takeaway.DistributedDb.Configuration.SyncTable> tables, SqlConnection connection, DbSyncScopeDescription scopeDesc)
        {
            var provision = new SqlSyncScopeProvisioning(connection, scopeDesc);

            try
            {
                Log.Info("[DistributedDb] Reorganize Scope Indexes [" + scopeDesc.ScopeName + "] Start", this);

                provision.CommandTimeout = 3600;
                provision.ReorganizeIndexes(connection, tables);

                Log.Info("[DistributedDb] Reorganize Scope Indexes [" + scopeDesc.ScopeName + "] End", this);
            }
            catch (Exception ex)
            {
                Log.Error("[DistributedDb] Reorganize Scope Indexes [" + scopeDesc.ScopeName + "] Error", ex, this);
            }
        }

        internal void TruncateTables(List<Sitecore.Takeaway.DistributedDb.Configuration.SyncTable> tables, SqlConnection connection, DbSyncScopeDescription scopeDesc)
        {
            var provision = new SqlSyncScopeProvisioning(connection, scopeDesc);

            try
            {
                Log.Info("[DistributedDb] TruncateTables Scope Tables [" + scopeDesc.ScopeName + "] Start", this);

                provision.CommandTimeout = 3600;
                provision.TruncateTables(connection, tables);

                Log.Info("[DistributedDb] TruncateTables Scope Tables [" + scopeDesc.ScopeName + "] End", this);
            }
            catch (Exception ex)
            {
                Log.Error("[DistributedDb] TruncateTables Scope Tables [" + scopeDesc.ScopeName + "] Error", ex, this);
            }
        }

        public abstract void Deprovision();

        internal void Deprovision(SyncDatabase db, SqlConnection connection, DbSyncScopeDescription scopeDesc)
        {
            this.GetDescriptionForTables(db.Tables, connection, ref scopeDesc);

            var provision = new SqlSyncScopeProvisioning(connection, scopeDesc);

            if (provision.ScopeExists(scopeDesc.ScopeName))
            {
                try
                {
                    var deprovision = new SqlSyncScopeDeprovisioning(connection);

                    Log.Info("[DistributedDb] Deprovision Scope [" + scopeDesc.ScopeName + "] Start", this);

                    deprovision.DeprovisionScope(scopeDesc.ScopeName);

                    Log.Info("[DistributedDb] Deprovision Scope [" + scopeDesc.ScopeName + "] End", this);
                }
                catch (Exception ex)
                {
                    Log.Error("[DistributedDb] Deprovision Scope [" + scopeDesc.ScopeName + "] Error", ex, this);
                }
            }
            else
                Log.Info("[DistributedDb] Deprovision Scope [" + scopeDesc.ScopeName + "] Skipped", this);
        }

        internal void DeprovisionStore(SqlConnection connection)
        {
            try
            {
                var deprovision = new SqlSyncScopeDeprovisioning(connection);

                Log.Info("[DistributedDb] Deprovision Store Start", this);

                deprovision.DeprovisionStore();

                Log.Info("[DistributedDb] Deprovision Store End", this);
            }
            catch (Exception ex)
            {
                Log.Error("[DistributedDb] Deprovision Store Error", ex, this);
            }
        }

        protected void GetDescriptionForTables(List<Sitecore.Takeaway.DistributedDb.Configuration.SyncTable> tables, SqlConnection connection, ref DbSyncScopeDescription scopeDesc)
        {
            foreach (var table in tables)
            {
                var tableDesc = SqlSyncDescriptionBuilder.GetDescriptionForTable(table.Name, connection);

                if (table.PrimaryKeys != null)
                {
                    foreach (var column in table.PrimaryKeys)
                    {
                        try
                        {
                            if (tableDesc.Columns[column] != null)
                                tableDesc.Columns[column].IsPrimaryKey = true;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("[DistributedDb] primary key column error", ex, this);
                        }
                    }
                }

                if (table.PrimaryKeysToRemove != null)
                {
                    foreach (var column in table.PrimaryKeysToRemove)
                    {
                        try
                        {
                            if (tableDesc.Columns[column] != null)
                                tableDesc.Columns[column].IsPrimaryKey = false;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("[DistributedDb] primary key column error", ex, this);
                        }
                    }
                }

                if (!scopeDesc.Tables.Contains(tableDesc)) scopeDesc.Tables.Add(tableDesc);
            }
        }
    }
}