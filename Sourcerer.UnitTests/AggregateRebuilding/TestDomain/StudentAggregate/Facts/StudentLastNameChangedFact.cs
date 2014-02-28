using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.UnitTests.AggregateRebuilding.TestDomain.StudentAggregate.Facts
{
    public class StudentLastNameChangedFact : FactAbout<Student>
    {
        public string LastName { get; set; }
    }
}