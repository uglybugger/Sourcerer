using System;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.UnitTests.AggregateRebuilding.TestDomain.StudentAggregate.Facts;

namespace Sourcerer.UnitTests.AggregateRebuilding.TestDomain.StudentAggregate
{
    [Serializable]
    public class Student : AggregateRoot
    {
        public static Student Create(string firstName, string lastName)
        {
            var id = Guid.NewGuid();
            var createdFact = new StudentCreatedFact {AggregateRootId = id};
            var student = new Student();
            student.Append(createdFact);
            student.Apply(createdFact);

            student.ChangeFirstName(firstName);
            student.ChangeLastName(lastName);

            return student;
        }

        protected Student()
        {
        }

        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public void Apply(StudentCreatedFact fact)
        {
            Id = fact.AggregateRootId;
        }

        public void ChangeFirstName(string firstName)
        {
            var fact = new StudentFirstNameChangedFact
                       {
                           AggregateRootId = Id,
                           FirstName = firstName,
                       };
            Append(fact);
            Apply(fact);
        }

        public void Apply(StudentFirstNameChangedFact fact)
        {
            FirstName = fact.FirstName;
        }

        public void ChangeLastName(string lastName)
        {
            var fact = new StudentLastNameChangedFact
                       {
                           AggregateRootId = Id,
                           LastName = lastName,
                       };

            Append(fact);
            Apply(fact);
        }

        public void Apply(StudentLastNameChangedFact fact)
        {
            LastName = fact.LastName;
        }
    }
}