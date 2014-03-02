using System.Xml.Serialization;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate.Facts
{
    [XmlRoot(Namespace = "Sourcerer.SchemaUpgradeTests.Domain.StudentAggregate.Facts.v1")]
    public class StudentCreatedFact : FactAbout<Student>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}