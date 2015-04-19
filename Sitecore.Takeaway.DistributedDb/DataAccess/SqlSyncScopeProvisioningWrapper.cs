using Microsoft.Synchronization.Data;
using Sitecore.Diagnostics;
using Sitecore.Takeaway.DistributedDb.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Sitecore.Takeaway.DistributedDb.DataAccess
{
    public class SqlSyncScopeProvisioningWrapper
    {
        public void UpdateTriggerScripts(string tableName, string cmdText, SqlConnection connection)
        {
            connection.Open();
            using (var sqlCommand = new SqlCommand(cmdText, connection))
            {
                //sqlCommand.CommandTimeout = 30;
                SyncTracer.Info("Update Trigger GETDATE() to GETUTCDATE() for table " + tableName);
                SyncTracerExtended.TraceCommandAndParameters((IDbCommand)sqlCommand);
                sqlCommand.ExecuteNonQuery();
            }
            connection.Close();
        }

        public void UpdateProcedureScripts(string tableName, string cmdText, SqlConnection connection)
        {
            connection.Open();
            using (var sqlCommand = new SqlCommand(cmdText, connection))
            {
                //sqlCommand.CommandTimeout = 30;
                SyncTracer.Info("Update Trigger GETDATE() to GETUTCDATE() for table " + tableName);
                SyncTracerExtended.TraceCommandAndParameters((IDbCommand)sqlCommand);
                sqlCommand.ExecuteNonQuery();
            }
            connection.Close();
        }

        public void RebuildTableIndexes(string tableName, string indexName, SqlConnection connection)
        {
            try
            {
                Log.Info("[DistributedDb] RebuildTableIndexes Start - " + indexName + " ON " + tableName, this);
                connection.Open();
                var command = @"
                    ALTER INDEX " + indexName + @" ON " + tableName + @" REBUILD;
                    UPDATE STATISTICS " + tableName + @" " + indexName + @";
                ";
                using (var sqlCommand = new SqlCommand(command, connection))
                {
                    //sqlCommand.CommandTimeout = 30;
                    SyncTracer.Info("Rebuild index table " + indexName + " ON " + tableName);
                    SyncTracerExtended.TraceCommandAndParameters((IDbCommand)sqlCommand);
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
                Log.Info("[DistributedDb] RebuildTableIndexes End - " + indexName + " ON " + tableName, this);
            }
            catch (Exception ex)
            {
                Log.Error("[DistributedDb] RebuildTableIndexes Error - " + indexName + " ON " + tableName, ex, this);
            }
        }

        public List<KeyValuePair<string, string>> GetFragmentedIndexes(string tableName, SqlConnection connection)
        {
            connection.Open();
            var tableIndexes = new List<KeyValuePair<string, string>>();
            var command = @"
                SELECT
	                OBJECT_NAME(ind.OBJECT_ID) AS TableName
	                , ind.name AS IndexName, indexstats.index_type_desc AS IndexType
	                , indexstats.avg_fragmentation_in_percent
                FROM
	                sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, NULL) indexstats INNER JOIN
	                sys.indexes ind  ON ind.object_id = indexstats.object_id
		                AND ind.index_id = indexstats.index_id
                WHERE
	                indexstats.avg_fragmentation_in_percent > 30
                    AND OBJECT_NAME(ind.OBJECT_ID) like '%" + tableName + @"%'
                ORDER BY
	                indexstats.avg_fragmentation_in_percent DESC
            ";
            using (var sqlCommand = new SqlCommand(command, connection))
            {
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var table = reader.GetString(reader.GetOrdinal("TableName"));
                        var indexName = reader.GetString(reader.GetOrdinal("IndexName"));
                        tableIndexes.Add(new KeyValuePair<string, string>(table, indexName));
                    }
                }
            }
            connection.Close();

            return tableIndexes;
        }

        public void ReorganizeTableIndexes(string tableName, string indexName, SqlConnection connection)
        {
            try
            {
                Log.Info("[DistributedDb] ReorganizeTableIndexes Start - " + indexName + " ON " + tableName, this);
                connection.Open();
                var command = @"
                    ALTER INDEX " + indexName + @" ON " + tableName + @" REORGANIZE;
                    UPDATE STATISTICS " + tableName + @" " + indexName + @";
                ";
                using (var sqlCommand = new SqlCommand(command, connection))
                {
                    SyncTracer.Info("Reorganize index table " + indexName + " ON " + tableName);
                    SyncTracerExtended.TraceCommandAndParameters((IDbCommand)sqlCommand);
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
                Log.Info("[DistributedDb] ReorganizeTableIndexes End - " + indexName + " ON " + tableName, this);
            }
            catch (Exception ex)
            {
                Log.Error("[DistributedDb] ReorganizeTableIndexes Error - " + indexName + " ON " + tableName, ex, this);
            }
        }

        public void TruncateTable(string tableName, SqlConnection connection)
        {
            try
            {
                Log.Info("[DistributedDb] TruncateTable Start - " + tableName, this);
                connection.Open();
                var command = @"
                    TRUNCATE TABLE " + tableName + @";
                ";
                using (var sqlCommand = new SqlCommand(command, connection))
                {
                    SyncTracer.Info("Truncate table " + tableName);
                    SyncTracerExtended.TraceCommandAndParameters((IDbCommand)sqlCommand);
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
                Log.Info("[DistributedDb] TruncateTable End - " + tableName, this);
            }
            catch (Exception ex)
            {
                Log.Error("[DistributedDb] TruncateTable Error - " + tableName, ex, this);
            }
        }
    }
}