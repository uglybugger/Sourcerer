using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate.Facts
{
    public class StudentLastNameChangedFact : FactAbout<Student>
    {
        public string LastName { get; set; }
    }
}