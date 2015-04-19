using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Takeaway.DistributedDb.Configuration;
using Sitecore.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Sitecore.Takeaway.DistributedDb.Core
{
    public class SyncConfiguration
    {
        public SyncConfiguration()
        {
        }

        public SyncConfiguration(XmlNodeList config, Dictionary<string, string> connectionStrings)
        {
            _databases = ParseSettings(config, connectionStrings);
        }

        private List<SyncDatabase> _databases;

        public List<SyncDatabase> Databases
        {
            get
            {
                return _databases;
            }
        }

        public string Client { get; set; }

        public string ClientConnectionString { get; set; }

        public string Server { get; set; }

        public string ServerConnectionString { get; set; }

        public void AddDatabase(string scope)
        {
            var db = Factory.CreateObject("distributedDb/databases/" + scope, true) as SyncDatabase;
            if (_databases == null) _databases = new List<SyncDatabase>();
            db.Scope = scope;
            if (string.IsNullOrEmpty(db.Client)) db.Client = this.Client;
            if (string.IsNullOrEmpty(db.ClientConnectionString)) db.ClientConnectionString = this.ClientConnectionString;
            if (string.IsNullOrEmpty(db.Server)) db.Server = this.Server;
            if (string.IsNullOrEmpty(db.ServerConnectionString)) db.ServerConnectionString = this.ServerConnectionString;
            _databases.Add(db);
        }

        private List<SyncDatabase> ParseSettings(XmlNodeList config, Dictionary<string, string> connectionStrings)
        {
            Assert.IsNotNull(config, "[DistributedDb] config is null");
            Assert.IsNotNull(connectionStrings, "[DistributedDb] connection is null");

            var dbs = new List<SyncDatabase>();
            foreach (XmlNode dbNode in config)
            {
                var db = new SyncDatabase();

                db.Scope = XmlUtil.GetAttribute("scope", dbNode);
                Assert.IsNotNullOrEmpty(db.Scope, "[DistributedDb] scope is missing");

                db.Client = XmlUtil.GetAttribute("client", dbNode);
                Assert.IsNotNullOrEmpty(db.Client, "[DistributedDb] client is missing");

                db.ClientConnectionString = connectionStrings[db.Client];
                Assert.IsNotNullOrEmpty(db.ClientConnectionString, "client connectionstring is missing");

                db.Server = XmlUtil.GetAttribute("server", dbNode);
                Assert.IsNotNullOrEmpty(db.Server, "[DistributedDb] server is missing");

                db.ServerConnectionString = connectionStrings[db.Server];
                Assert.IsNotNullOrEmpty(db.ServerConnectionString, "server connectionstring is missing");

                foreach (XmlNode tableNode in dbNode.ChildNodes)
                {
                    var table = new SyncTable();
                    table.Name = XmlUtil.GetAttribute("name", tableNode);
                    table.Database = db;
                    Assert.IsNotNullOrEmpty(table.Name, "[DistributedDb] table name is missing");
                    if (db.Tables == null) db.Tables = new List<SyncTable>();
                    db.Tables.Add(table);
                }

                Assert.IsTrue(db.Tables.Count > 0, "[DistributedDb] no tables found");
                dbs.Add(db);
            }
            Assert.IsTrue(dbs.Count > 0, "[DistributedDb] no database found");
            return dbs;
        }
    }
}