using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate.Facts
{
    public class StudentFirstNameChangedFact : FactAbout<Student>
    {
        public string FirstName { get; set; }
    }
}