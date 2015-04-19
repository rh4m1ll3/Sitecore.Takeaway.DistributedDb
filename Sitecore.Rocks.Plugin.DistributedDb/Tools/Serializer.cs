using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace Sitecore.Rocks.Plugin.DistributedDb.Tools
{
    public class Serializer
    {
        public T Deserialize<T>(string data)
        {
            return (T)this.Deserialize(data, typeof(T));
        }

        public virtual object Deserialize(string data, Type resultType)
        {
            XmlDictionaryReader jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(data), XmlDictionaryReaderQuotas.Max);
            DataContractJsonSerializer contractJsonSerializer = new DataContractJsonSerializer(resultType);
            try
            {
                return contractJsonSerializer.ReadObject(jsonReader);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string Serialize<T>(T data)
        {
            return this.Serialize((object)data);
        }

        public virtual string Serialize(object data)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                new DataContractJsonSerializer(data.GetType()).WriteObject((Stream)memoryStream, data);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}