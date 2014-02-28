using System;
using System.Xml;
using System.Xml.Serialization;

namespace Sourcerer.DomainConcepts
{
    [Serializable]
    public class UnitOfWorkProperties : IComparable<UnitOfWorkProperties>
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

        /// <see
        ///     cref="https://connect.microsoft.com/VisualStudio/feedback/details/288349/datetimeoffset-is-not-serialized-by-a-xmlserializer" />
        [XmlElement("FactTimestamp")]
        public string FactTimestampWorkaround
        {
            get { return XmlConvert.ToString(FactTimestamp); }
            set { FactTimestamp = XmlConvert.ToDateTimeOffset(value); }
        }

        public int CompareTo(UnitOfWorkProperties other)
        {
            if (FactTimestamp < other.FactTimestamp) return -1;
            if (FactTimestamp > other.FactTimestamp) return 1;

            var uowIdComparison = UnitOfWorkId.CompareTo(other.UnitOfWorkId);
            if (uowIdComparison != 0) return uowIdComparison;

            return SequenceNumber.CompareTo(other.SequenceNumber);
        }
    }
}