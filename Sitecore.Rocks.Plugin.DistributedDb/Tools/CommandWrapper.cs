using Sitecore.Rocks.Plugin.DistributedDb.Commands;


using Sitecore.VisualStudio.ContentTrees;
using Sitecore.VisualStudio.ContentTrees.Items;
using Sitecore.VisualStudio.Data;
using Sitecore.VisualStudio.Data.DataServices;
using Sitecore.VisualStudio.Shell.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Sitecore.Rocks.Plugin.DistributedDb.Tools
{
    public class CommandWrapper
    {
        public static string GetResultValue(string xml, string xPath)
        {
            if (string.IsNullOrEmpty(xml)) return string.Empty;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode xmlNode = xmlDocument.SelectSingleNode(xPath);
            return xmlNode.InnerText;
        }

        public static List<string> GetResultValues(string xml, string tagName)
        {
            if (string.IsNullOrEmpty(xml)) return new List<string>();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            var xmlNode = xmlDocument.GetElementsByTagName(tagName);
            var result = new List<string>();
            for (int i = 0; i < xmlNode.Count; i++)
            {
                result.Add(xmlNode[i].InnerText);
            }
            return result;
        }

        public static DbSyncPipelineResult GetDbSyncPipelineResult(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return null;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("//result/pipelineResult");
            XmlNode childNode = xmlNode.ChildNodes[0];
            if (childNode is XmlCDataSection)
            {
                XmlCDataSection cdataSection = childNode as XmlCDataSection;
                if (cdataSection != null && !string.IsNullOrEmpty(cdataSection.Value))
                    return new Serializer().Deserialize<DbSyncPipelineResult>(cdataSection.Value);
            }
            return null;
        }

        public static bool CanExecute(object parameter)
        {
            ContentTreeContext contentTreeContext = parameter as ContentTreeContext;
            if (contentTreeContext == null || Enumerable.Count<BaseTreeViewItem>(contentTreeContext.SelectedItems) != 1)
                return false;
            SiteTreeViewItem siteTreeViewItem = Enumerable.FirstOrDefault<BaseTreeViewItem>(contentTreeContext.SelectedItems) as SiteTreeViewItem;
            return siteTreeViewItem != null;
        }

        public static void Execute(object parameter, string pipeline)
        {
            Execute(parameter, pipeline, "Sitecore.Rocks.Server.Requests.DistributedDb.ExecuteDbSyncPipeline, Sitecore.Rocks.Server.DistributedDb");
        }

        public static void Execute(object parameter, string pipeline, string typeName)
        {
            if (!CanExecute(parameter)) return;

            var context = parameter as ISiteSelectionContext;

            if (context == null)
                return;

            var output = new VisualStudioOutputHost();
            var sitecoreInstance = context.Site.Name;

            output.Show();
            try
            {
                output.Write(DateTime.Now.ToString("hh:mm:ss.fff") + " [DistributedDb] Command " + pipeline + " Start - " + sitecoreInstance);
                context.Site.DataService.ExecuteAsync(typeName, completedExecute, context.Site.Name, pipeline);
                output.Write(DateTime.Now.ToString("hh:mm:ss.fff") + " [DistributedDb] Command " + pipeline + " End - " + sitecoreInstance);
            }
            catch (Exception ex)
            {
                output.Write(DateTime.Now.ToString("hh:mm:ss.fff") + " [DistributedDb] Command " + pipeline + " Error - " + sitecoreInstance + "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }

        public static ExecuteCompleted completedExecute = delegate(string response, ExecuteResult result)
        {
            var output = new VisualStudioOutputHost();
            output.Show();

            try
            {
                if (!DataService.HandleExecute(response, result) || string.IsNullOrEmpty(response))
                {
                    throw new Exception("cannot handle response or the response is empty");
                }

                ContentTree activeContentTree = Sitecore.VisualStudio.UI.ActiveContext.ActiveContentTree;

                if (activeContentTree != null)
                {
                    var pipeline = CommandWrapper.GetResultValue(response, "//result/pipeline");
                    var pipelineResult = CommandWrapper.GetDbSyncPipelineResult(response);
                    var sitecoreInstance = result.DataService.Connection.HostName;

                    output.Write(DateTime.Now.ToString("hh:mm:ss.fff") + " [DistributedDb] Response " + pipeline + " Start - " + sitecoreInstance);

                    if (pipelineResult != null && pipelineResult.Databases != null)
                    {
                        foreach (var db in pipelineResult.Databases)
                        {
                            output.Write(DateTime.Now.ToString("hh:mm:ss.fff") + " [DistributedDb] Result " + pipeline + " Database [" + db + "] - " + sitecoreInstance);
                        }
                    }

                    output.Write(DateTime.Now.ToString("hh:mm:ss.fff") + " [DistributedDb] Response " + pipeline + " End - " + sitecoreInstance);
                }
            }
            catch (Exception ex)
            {
                output.Write(DateTime.Now.ToString("hh:mm:ss.fff") + " [DistributedDb] Response Error " + "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        };
    }
}