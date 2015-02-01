using System;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.SchemaUpgradeTests.v1.Domain.StudentAggregate.Facts;

namespace Sourcerer.SchemaUpgradeTests.v1.Domain.StudentAggregate
{
    [Serializable]
    public class Student : AggregateRoot
    {
        public static Student Create(string firstName, string lastName)
        {
            var id = Guid.NewGuid();
            var createdFact = new StudentCreatedFact
                              {
                                  AggregateRootId = id,
                                  FirstName = firstName,
                                  LastName = lastName,
                              };
            var student = new Student();
            student.Append(createdFact);
            student.Apply(createdFact);

            return student;
        }

        protected Student()
        {
        }

        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Guid AddressId { get; private set; }

        public void Apply(StudentCreatedFact fact)
        {
            Id = fact.AggregateRootId;
            FirstName = fact.FirstName;
            LastName = fact.LastName;
        }

        public void ChangeAddress(Guid addressId)
        {
            var fact = new StudentChangedAddressFact
                       {
                           AggregateRootId = Id,
                           AddressId = addressId,
                       };

            Append(fact);
            Apply(fact);
        }

        public void Apply(StudentChangedAddressFact fact)
        {
            AddressId = fact.AddressId;
        }
    }
}