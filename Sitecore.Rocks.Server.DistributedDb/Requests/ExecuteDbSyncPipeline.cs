namespace Sitecore.Rocks.Server.Requests.DistributedDb
{
    using Sitecore.Common;
    using Sitecore.Diagnostics;
    using Sitecore.Jobs;
    using Sitecore.Pipelines;
    using Sitecore.Takeaway.DistributedDb.Core;
    using Sitecore.Takeaway.DistributedDb.Processors;
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;

    public class ExecuteDbSyncPipeline
    {
        public void Process(string pipeline, DbSyncPipelineArgs pipelineArgs)
        {
            if (pipeline.StartsWith("distributedDb."))
                pipeline = pipeline.Replace("distributedDb.", "");

            Log.Info("[DistributedDb] Pipeline Job " + pipeline + " Start", this);

            CorePipeline.Run("distributedDb." + pipeline, pipelineArgs);

            Log.Info("[DistributedDb] Pipeline Job  " + pipeline + " End", this);
        }

        public string Execute(string sitecoreInstance, string pipeline)
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("result");

            try
            {
                Log.Info("[DistributedDb] Request " + pipeline + " Start - " + sitecoreInstance, this);

                var pipelineArgs = new DbSyncPipelineArgs() { Server = sitecoreInstance };

                JobOptions options = new JobOptions("[DistributedDb] Job " + pipeline, "DistributedDb", sitecoreInstance, this, "Process", new object[] { pipeline, pipelineArgs });
                Job job = JobManager.Start(options);

                output.WriteStartElement("pipeline");
                output.WriteString(pipeline);
                output.WriteEndElement();

                output.WriteStartElement("pipelineResult");

                var result = new DbSyncPipelineResult();
                var syncManager = new DbSyncManager();

                result.Pipeline = pipeline;

                if (syncManager.Server.Databases != null)
                {
                    result.Databases = syncManager.Server.Databases.Select(db => db.Scope).ToList();
                }
                else
                {
                    throw new Exception("no databases found");
                }

                output.WriteCData(new Serializer().Serialize<DbSyncPipelineResult>(result));
                output.WriteEndElement();

                Log.Info("[DistributedDb] Request " + pipeline + " End - " + sitecoreInstance, this);
            }
            catch (Exception ex)
            {
                Log.Error("[DistributedDb] Request " + pipeline + " Error - " + sitecoreInstance, ex, this);
                output.WriteStartElement("error");
                output.WriteString(ex.Message + " - " + ex.StackTrace);
                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}