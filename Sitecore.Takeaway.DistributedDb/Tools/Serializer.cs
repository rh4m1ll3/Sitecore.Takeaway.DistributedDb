using Sitecore.Diagnostics;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace Sitecore.Takeaway.DistributedDb.Tools
{
    public class Serializer
    {
        public T Deserialize<T>(string data)
        {
            Assert.ArgumentNotNull((object)data, "data");
            return (T)this.Deserialize(data, typeof(T));
        }

        public virtual object Deserialize(string data, Type resultType)
        {
            Assert.ArgumentNotNull((object)data, "data");
            Assert.ArgumentNotNull((object)resultType, "resultType");
            XmlDictionaryReader jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(data), XmlDictionaryReaderQuotas.Max);
            DataContractJsonSerializer contractJsonSerializer = new DataContractJsonSerializer(resultType);
            try
            {
                return Assert.ResultNotNull<object>(contractJsonSerializer.ReadObject(jsonReader));
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("[DistributedDb] Error when deserializing the object.\nData: {0}\nType: {1}", (object)data, (object)resultType.FullName), ex, (object)this);
                throw;
            }
        }

        public string Serialize<T>(T data)
        {
            return this.Serialize((object)data);
        }

        public virtual string Serialize(object data)
        {
            Assert.ArgumentNotNull(data, "data");
            using (MemoryStream memoryStream = new MemoryStream())
            {
                new DataContractJsonSerializer(data.GetType()).WriteObject((Stream)memoryStream, data);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        //public void test()
        //{
        //    var template = Sitecore.Data.Database.GetDatabase("master").Templates[new Sitecore.Data.ID("{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}")];
        //    var events = Sitecore.Data.Database.GetDatabase("master").Items["/sitecore/content/Esplanade/Storage/Events/2015"];
        //    var folder = Sitecore.Context.Database.Items["/sitecore/content/Esplanade/Storage/Events/2015/A"];

        //    if (events != null && folder == null && template != null)
        //    {
        //        folder = events.Add("A", template);
        //    }

        //    var eventtemplate = Sitecore.Data.Database.GetDatabase("master").Templates[new Sitecore.Data.ID("{A4A3C032-A7E4-453A-A7BF-0C03861BEC1C}")];
        //    var newevent = folder.Add("", eventtemplate);
        //    try
        //    {
        //        //Page	Festivals	Venues	Advisories	Genre	Featured	FreeProgramme	Audience	Event Type	Ticketing_url

        //        newevent.Editing.BeginEdit();
        //        newevent.Fields["Title"].Value = employeeId;
        //        newevent.Fields["Festival"].Value = employeeName;
        //        newevent.Fields["Venue"].Value = gender;
        //        newevent.Fields["Advisories"].Value = location;
        //        newevent.Fields["Primary Genre"].Value = location;
        //        newevent.Fields["Is Featured"].Value = location;
        //        newevent.Fields["Is Free"].Value = location;
        //        newevent.Fields["Target Audience"].Value = location;
        //        newevent.Fields["Event Type"].Value = location;
        //        newevent.Fields["Buy Ticket Link"].Value = @"<link target="" text="Buy Ticket" anchor="" url="http://google.com" linktype="external" />";
        //        newevent.Editing.EndEdit();
        //    }
        //    catch (Exception ex)
        //    {
        //        newevent.Editing.CancelEdit();
        //    }

        //}
    }
}