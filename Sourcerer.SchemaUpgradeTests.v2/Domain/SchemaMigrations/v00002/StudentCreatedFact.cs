using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.SchemaMigrations.v00002
{
    [Fact("{A6260704-B9EC-4C74-86FD-6A4C5722D95E}")]
    public class StudentCreatedFact : FactAbout<Student>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}