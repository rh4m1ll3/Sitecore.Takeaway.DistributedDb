using System.Collections.Generic;
using System.Xml;

namespace Sitecore.Takeaway.DistributedDb.Configuration
{
    public class SyncDatabase
    {
        public string Scope { get; set; }

        public string Client { get; set; }

        public string ClientConnectionString { get; set; }

        public string Server { get; set; }

        public string ServerConnectionString { get; set; }

        public List<SyncTable> Tables { get; set; }

        public void AddTable(XmlNode tables)
        {
            var table = tables.SelectNodes("table");

            for (int i = 0; i < table.Count; i++)
            {
                var primaryKeys = new List<string>();
                var primaryKeysToRemove = new List<string>();

                var name = table[i].InnerText;
                var keys = table[i].Attributes["primaryKeys"];
                var removekeys = table[i].Attributes["removeAutoPKFromIndexes"];

                if (keys != null)
                {
                    foreach (var pk in keys.Value.Split(','))
                        if (!primaryKeys.Contains(pk)) primaryKeys.Add(pk);
                }

                if (removekeys != null)
                {
                    foreach (var pk in removekeys.Value.Split(','))
                        if (!primaryKeysToRemove.Contains(pk)) primaryKeysToRemove.Add(pk);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    if (this.Tables == null) this.Tables = new List<SyncTable>();
                    this.Tables.Add(new SyncTable { Name = name, PrimaryKeys = primaryKeys, PrimaryKeysToRemove = primaryKeysToRemove });
                }
            }
        }
    }
}