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
                return contractJsonSerializer.ReadObject(jsonReader);
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
    }
}