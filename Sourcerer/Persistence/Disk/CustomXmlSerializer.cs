using System;
using System.IO;
using System.Xml.Serialization;
using Sourcerer.Infrastructure;

namespace Sourcerer.Persistence.Disk
{
    internal class CustomXmlSerializer : ICustomSerializer
    {
        private readonly IFactTypesProvider _factTypesProvider;
        private readonly Lazy<XmlSerializer> _serializer;

        public CustomXmlSerializer(IFactTypesProvider factTypesProvider)
        {
            _factTypesProvider = factTypesProvider;
            _serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof (SerializationWrapper), _factTypesProvider.FactTypes));
        }

        public void Serialize(Stream stream, object item)
        {
            var wrapper = new SerializationWrapper {Inner = item};
            _serializer.Value.Serialize(stream, wrapper);
        }

        public T Deserialize<T>(FileStream stream)
        {
            var wrapper = (SerializationWrapper) _serializer.Value.Deserialize(stream);
            return (T) wrapper.Inner;
        }
    }
}