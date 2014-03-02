using System;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate
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

        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }

        public void Apply(StudentCreatedFact fact)
        {
            Id = fact.AggregateRootId;
            FirstName = fact.FirstName;
            LastName = fact.LastName;
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

        public void ChangeAddress(string streetAddress, string suburb, string state, string postcode)
        {
            var fact = new StudentChangedAddressFact
                       {
                           StreetAddress = streetAddress,
                           Suburb = suburb,
                           State = state,
                           PostCode = postcode,
                       };

            Append(fact);
            Apply(fact);
        }

        public void Apply(StudentChangedAddressFact fact)
        {
            StreetAddress = fact.StreetAddress;
            Suburb = fact.Suburb;
            State = fact.State;
            PostCode = fact.PostCode;
        }
    }
}