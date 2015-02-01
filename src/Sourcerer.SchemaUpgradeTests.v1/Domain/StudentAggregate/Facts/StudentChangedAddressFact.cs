using System;
using System.Xml.Serialization;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v1.Domain.StudentAggregate.Facts
{
    [XmlRoot(Namespace = "Sourcerer.SchemaUpgradeTests.Domain.StudentAggregate.Facts.v2")]
    public class StudentChangedAddressFact : FactAbout<Student>
    {
        public Guid AddressId { get; set; }
    }
}