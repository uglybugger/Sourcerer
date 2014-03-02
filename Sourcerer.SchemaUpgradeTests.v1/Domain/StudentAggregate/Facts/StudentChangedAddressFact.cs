using System.Xml.Serialization;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v1.Domain.StudentAggregate.Facts
{
    [XmlRoot(Namespace = "Sourcerer.SchemaUpgradeTests.Domain.StudentAggregate.Facts.v1", ElementName = "StudentChangedAddressFact")]
    public class StudentChangedAddressFact : FactAbout<Student>
    {
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
    }
}