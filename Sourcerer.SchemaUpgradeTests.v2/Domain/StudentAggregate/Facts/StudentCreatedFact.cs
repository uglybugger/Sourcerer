using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate.Facts
{
    public class StudentCreatedFact : FactAbout<Student>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}