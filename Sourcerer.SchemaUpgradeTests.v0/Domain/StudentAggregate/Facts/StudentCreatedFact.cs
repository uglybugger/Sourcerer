using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v0.Domain.StudentAggregate.Facts
{
    [Fact("{A6260704-B9EC-4C74-86FD-6A4C5722D95E}")]
    public class StudentCreatedFact : FactAbout<Student>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}