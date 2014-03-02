using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Sourcerer.Infrastructure;

namespace Sourcerer.Persistence.Disk
{
    internal class CustomXmlSerializer : ICustomSerializer
    {
        private readonly ITypesProvider _typesProvider;
        private readonly Lazy<XmlSerializer> _serializer;

        public CustomXmlSerializer(ITypesProvider typesProvider)
        {
            _typesProvider = typesProvider;
            _serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof (SerializationWrapper), _typesProvider.FactTypes));
        }

        [Obsolete]
        public void Serialize(Stream stream, object item)
        {
            var wrapper = new SerializationWrapper {Inner = item};
            _serializer.Value.Serialize(stream, wrapper);
        }

        public byte[] Serialize(object o)
        {
            using (var ms = new MemoryStream())
            {
                Serialize(ms, o);
                var result = ms.GetBuffer().ToArray();
                return result;
            }
        }

        [Obsolete]
        public T Deserialize<T>(Stream stream)
        {
            var wrapper = (SerializationWrapper) _serializer.Value.Deserialize(stream);
            return (T) wrapper.Inner;
        }

        public T Deserialize<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Deserialize<T>(ms);
            }
        }
    }
}