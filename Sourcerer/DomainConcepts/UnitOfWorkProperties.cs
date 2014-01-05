using System;
using System.Xml;
using System.Xml.Serialization;

namespace Sourcerer.DomainConcepts
{
    [Serializable]
    public class UnitOfWorkProperties
    {
        public UnitOfWorkProperties()
        {
        }

        public UnitOfWorkProperties(Guid unitOfWorkId, int sequenceNumber, DateTimeOffset factTimestamp)
        {
            UnitOfWorkId = unitOfWorkId;
            SequenceNumber = sequenceNumber;
            FactTimestamp = factTimestamp;
        }

        public Guid UnitOfWorkId { get; set; }

        public int SequenceNumber { get; set; }

        [XmlIgnore]
        public DateTimeOffset FactTimestamp { get; set; }

        /// <see cref="https://connect.microsoft.com/VisualStudio/feedback/details/288349/datetimeoffset-is-not-serialized-by-a-xmlserializer"/>
        [XmlElement("FactTimestamp")]
        public string FactTimestampWorkaround
        {
            get { return XmlConvert.ToString(FactTimestamp); }
            set { FactTimestamp = XmlConvert.ToDateTimeOffset(value); }
        }
    }
}