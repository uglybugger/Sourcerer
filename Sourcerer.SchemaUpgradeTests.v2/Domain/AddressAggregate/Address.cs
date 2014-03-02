using System;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate
{
    public class Address : AggregateRoot
    {
        protected Address()
        {
        }

        public static Address Create(string streetAddress, string suburb, string state, string postcode)
        {
            var fact = new AddressCreatedFact
                       {
                           AggregateRootId = Guid.NewGuid(),
                           StreetAddress = streetAddress,
                           Suburb = suburb,
                           State = state,
                           PostCode = postcode,
                       };

            var address = new Address();
            address.Append(fact);
            address.Apply(fact);
            return address;
        }

        private void Apply(AddressCreatedFact fact)
        {
            Id = fact.AggregateRootId;
            StreetAddress = fact.StreetAddress;
            Suburb = fact.Suburb;
            State = fact.State;
            PostCode = fact.PostCode;
        }

        public string StreetAddress { get; private set; }
        public string Suburb { get; private set; }
        public string State { get; private set; }
        public string PostCode { get; private set; }
    }
}