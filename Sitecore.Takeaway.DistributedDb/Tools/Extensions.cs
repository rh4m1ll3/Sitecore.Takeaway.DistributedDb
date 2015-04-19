using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Sitecore.Diagnostics;
using Sitecore.Takeaway.DistributedDb.DataAccess;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace Sitecore.Takeaway.DistributedDb.Tools
{
    public static class Extensions
    {
        public static string[] GetUpdateTriggerScripts(this SqlSyncTableProvisioning table)
        {
            Assert.IsNotNull(table, "table is null");
            table.CreateProcedures = DbSyncCreationOption.Skip;
            table.CreateProceduresForAdditionalScope = DbSyncCreationOption.Skip;
            table.CreateTable = DbSyncCreationOption.Skip;
            table.CreateTrackingTable = DbSyncCreationOption.Skip;
            table.CreateTriggers = DbSyncCreationOption.Create;
            var script = table.Script();
            script = script.Replace("CREATE TRIGGER", "ALTER TRIGGER");
            script = script.Replace("GETDATE()", "GETUTCDATE()");
            script = script.Replace("GO\n", "|");
            return script.Split('|');
        }

        public static string[] GetUpdateProcedureScripts(this SqlSyncTableProvisioning table)
        {
            Assert.IsNotNull(table, "table is null");
            table.CreateProcedures = DbSyncCreationOption.Create;
            table.CreateProceduresForAdditionalScope = DbSyncCreationOption.Skip;
            table.CreateTable = DbSyncCreationOption.Skip;
            table.CreateTrackingTable = DbSyncCreationOption.Skip;
            table.CreateTriggers = DbSyncCreationOption.Skip;
            var script = table.Script();
            script = script.Replace("CREATE PROCEDURE", "ALTER PROCEDURE");
            script = script.Replace("INSERT INTO [scope_config]", "--INSERT INTO [scope_config]");
            script = script.Replace("CREATE TYPE", "--CREATE TYPE");
            script = script.Replace("GETDATE()", "GETUTCDATE()");
            script = script.Replace("GO\n", "|");
            return script.Split('|');
        }

        public static Dictionary<string, string> ToDictionary(this ConnectionStringSettingsCollection connectionStrings)
        {
            var result = new Dictionary<string, string>();
            foreach (ConnectionStringSettings c in connectionStrings)
            {
                result.Add(c.Name, c.ConnectionString);
            }
            return result;
        }

        public static void ApplyTriggerAndProcedureUpdates(this SqlSyncScopeProvisioning provision, SqlConnection connection)
        {
            Assert.IsNotNull(provision, "[DistributedDb] provision is null");

            var scopeProvisioningWrapper = new SqlSyncScopeProvisioningWrapper();

            foreach (var table in provision.Tables)
            {
                foreach (var trigger in table.GetUpdateTriggerScripts())
                {
                    scopeProvisioningWrapper.UpdateTriggerScripts(table.LocalName, trigger, connection);
                }

                foreach (var procedure in table.GetUpdateProcedureScripts())
                {
                    scopeProvisioningWrapper.UpdateProcedureScripts(table.LocalName, procedure, connection);
                }
            }
        }

        public static void RebuildIndexes(this SqlSyncScopeProvisioning provision, SqlConnection connection, List<Sitecore.Takeaway.DistributedDb.Configuration.SyncTable> tables)
        {
            Assert.IsNotNull(provision, "[DistributedDb] provision is null");

            var scopeProvisioningWrapper = new SqlSyncScopeProvisioningWrapper();

            foreach (var table in tables)
            {
                foreach (var idx in scopeProvisioningWrapper.GetFragmentedIndexes(table.Name, connection))
                {
                    scopeProvisioningWrapper.RebuildTableIndexes(idx.Key, idx.Value, connection);
                }
            }
        }

        public static void ReorganizeIndexes(this SqlSyncScopeProvisioning provision, SqlConnection connection, List<Sitecore.Takeaway.DistributedDb.Configuration.SyncTable> tables)
        {
            Assert.IsNotNull(provision, "[DistributedDb] provision is null");

            var scopeProvisioningWrapper = new SqlSyncScopeProvisioningWrapper();

            foreach (var table in tables)
            {
                foreach (var idx in scopeProvisioningWrapper.GetFragmentedIndexes(table.Name, connection))
                {
                    scopeProvisioningWrapper.ReorganizeTableIndexes(idx.Key, idx.Value, connection);
                }
            }
        }

        public static void TruncateTables(this SqlSyncScopeProvisioning provision, SqlConnection connection, List<Sitecore.Takeaway.DistributedDb.Configuration.SyncTable> tables)
        {
            Assert.IsNotNull(provision, "[DistributedDb] provision is null");

            var scopeProvisioningWrapper = new SqlSyncScopeProvisioningWrapper();

            foreach (var table in tables)
            {
                scopeProvisioningWrapper.TruncateTable(table.Name, connection);
            }
        }
    }
}