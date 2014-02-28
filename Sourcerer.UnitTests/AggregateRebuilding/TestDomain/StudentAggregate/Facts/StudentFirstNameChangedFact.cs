using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.UnitTests.AggregateRebuilding.TestDomain.StudentAggregate.Facts
{
    public class StudentFirstNameChangedFact : FactAbout<Student>
    {
        public string FirstName { get; set; }
    }
}