using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate.Facts
{
    [Fact("{BC08F046-12B9-443B-9B58-886218630209}")]
    public class StudentCreatedFact : FactAbout<Student>
    {
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
    }
}