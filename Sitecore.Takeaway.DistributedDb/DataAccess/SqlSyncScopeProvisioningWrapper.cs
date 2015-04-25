using Microsoft.Synchronization.Data;
using Sitecore.Diagnostics;
using Sitecore.Takeaway.DistributedDb.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Sitecore.Takeaway.DistributedDb.DataAccess
{

    public class SyncDbFragmentationInfo
    {
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public int Fragmentation { get; set; }
    }

    public class SqlSyncScopeProvisioningWrapper
    {
        public void UpdateTriggerScripts(string tableName, string cmdText, SqlConnection connection)
        {
            try
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
            } catch (Exception ex)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error("[DistributedDb] UpdateTriggerScripts Error", ex, this);
            }
        }

        public void UpdateProcedureScripts(string tableName, string cmdText, SqlConnection connection)
        {
            try
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
            catch (Exception ex)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error("[DistributedDb] UpdateProcedureScripts Error", ex, this);
            }
        }

        public void RebuildTableIndexes(string tableName, string indexName, SqlConnection connection)
        {
            try
            {
                Log.Info("[DistributedDb] RebuildTableIndexes Start - [" + tableName + "] ON " + indexName, this);
                connection.Open();
                var command = @"
                    ALTER INDEX " + indexName + @" ON " + tableName + @" REBUILD WITH (FILLFACTOR = 80);
                ";
                using (var sqlCommand = new SqlCommand(command, connection))
                {
                    //sqlCommand.CommandTimeout = 30;
                    SyncTracer.Info("Rebuild index table " + indexName + " ON " + tableName);
                    SyncTracerExtended.TraceCommandAndParameters((IDbCommand)sqlCommand);
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
                Log.Info("[DistributedDb] RebuildTableIndexes End - [" + tableName + "] ON " + indexName, this);
            }
            catch (Exception ex)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error("[DistributedDb] RebuildTableIndexes Error - [" + tableName + "] ON " + indexName, ex, this);
            }
        }

        public List<SyncDbFragmentationInfo> GetFragmentedIndexes(string tableName, SqlConnection connection)
        {
            try
            {
                connection.Open();
                var tableIndexes = new List<SyncDbFragmentationInfo>();
                var command = @"
                SELECT
	                OBJECT_NAME(ind.OBJECT_ID) AS TableName
	                , ind.name AS IndexName
                    , indexstats.index_type_desc AS IndexType
	                , CONVERT(int,indexstats.avg_fragmentation_in_percent) AS Fragmentation
                FROM
	                sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, NULL) indexstats INNER JOIN
	                sys.indexes ind  ON ind.object_id = indexstats.object_id
		                AND ind.index_id = indexstats.index_id
                WHERE
	                indexstats.avg_fragmentation_in_percent > 10
                    AND indexstats.page_count >= 1000
                    AND OBJECT_NAME(ind.OBJECT_ID) like '" + tableName + @"%'
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
                            var fragmentation = reader.GetInt32(reader.GetOrdinal("Fragmentation"));
                            tableIndexes.Add(new SyncDbFragmentationInfo() { TableName = table, IndexName = indexName, Fragmentation = fragmentation });
                        }
                    }
                }
                connection.Close();

                return tableIndexes;

            } catch (Exception ex)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error("[DistributedDb] GetFragmentedIndexes Error", ex, this);
            }
            return new List<SyncDbFragmentationInfo>(); ;

        }

        public void ReorganizeTableIndexes(string tableName, string indexName, SqlConnection connection)
        {
            try
            {
                Log.Info("[DistributedDb] ReorganizeTableIndexes Start - [" + tableName + "] ON " + indexName, this);
                connection.Open();
                var command = @"
                    ALTER INDEX " + indexName + @" ON " + tableName + @" REORGANIZE;
                ";
                using (var sqlCommand = new SqlCommand(command, connection))
                {
                    SyncTracer.Info("Reorganize index table " + indexName + " ON " + tableName);
                    SyncTracerExtended.TraceCommandAndParameters((IDbCommand)sqlCommand);
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
                Log.Info("[DistributedDb] ReorganizeTableIndexes End - [" + tableName + "] ON " + indexName, this);
            }
            catch (Exception ex)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error("[DistributedDb] ReorganizeTableIndexes Error - [" + tableName + "] ON " + indexName, ex, this);
            }
        }

        public void TruncateTable(string tableName, SqlConnection connection)
        {
            try
            {
                Log.Info("[DistributedDb] TruncateTable Start - [" + tableName + "]", this);
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
                Log.Info("[DistributedDb] TruncateTable End - " + tableName + "]", this);
            }
            catch (Exception ex)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error("[DistributedDb] TruncateTable Error - " + tableName + "]", ex, this);
            }
        }
    }
}