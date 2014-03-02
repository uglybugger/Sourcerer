using System.Xml.Serialization;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate.Facts
{
    [XmlRoot(Namespace = "Sourcerer.SchemaUpgradeTests.Domain.StudentAggregate.Facts.v1")]
    public class AddressCreatedFact : FactAbout<Address>
    {
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
    }
}