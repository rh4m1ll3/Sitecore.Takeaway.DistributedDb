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
        private string _pipleline;

        public string Pipeline
        {
            get
            {
                return _pipleline;
            }
        }

        public ExecuteDbSyncPipeline()
        {

        }

        public ExecuteDbSyncPipeline(string pipeline)
        {
            _pipleline = pipeline;
        }

        public void Run()
        {
            Log.Info("[DistributedDb] Agent " + this.Pipeline + " Start - " + Context.Site.Name, this);

            Assert.IsNotNullOrEmpty(this.Pipeline, "pipeline is null or empty");

            StartJob(Context.Site.Name, this.Pipeline);

            Log.Info("[DistributedDb] Agent " + this.Pipeline + " End - " + Context.Site.Name, this);

        }

        public void Process(string pipeline, DbSyncPipelineArgs pipelineArgs)
        {
            if (pipeline.StartsWith("distributedDb."))
                pipeline = pipeline.Replace("distributedDb.", "");

            Log.Info("[DistributedDb] Pipeline Job " + pipeline + " Start", this);

            CorePipeline.Run("distributedDb." + pipeline, pipelineArgs);

            Log.Info("[DistributedDb] Pipeline Job  " + pipeline + " End", this);
        }


        public void StartJob(string sitecoreInstance, string pipeline)
        {
            Log.Info("[DistributedDb] Job " + pipeline + " Start", this);

            var pipelineArgs = new DbSyncPipelineArgs() { Server = sitecoreInstance };

            JobOptions options = new JobOptions("[DistributedDb] Job " + pipeline, "DistributedDb", sitecoreInstance, this, "Process", new object[] { pipeline, pipelineArgs });
            Job job = JobManager.Start(options);

            Log.Info("[DistributedDb] Job " + pipeline + " End", this);

        }

        public string Execute(string sitecoreInstance, string pipeline)
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("result");

            if (string.IsNullOrEmpty(sitecoreInstance))
                sitecoreInstance = Context.Site.Name;

            try
            {
                Log.Info("[DistributedDb] Rocks Request " + pipeline + " Start - " + sitecoreInstance, this);

                StartJob(sitecoreInstance, pipeline);

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

                Log.Info("[DistributedDb] Rocks Request " + pipeline + " End - " + sitecoreInstance, this);
            }
            catch (Exception ex)
            {
                Log.Error("[DistributedDb] Rocks Request " + pipeline + " Error - " + sitecoreInstance, ex, this);
                output.WriteStartElement("error");
                output.WriteString(ex.Message + " - " + ex.StackTrace);
                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}