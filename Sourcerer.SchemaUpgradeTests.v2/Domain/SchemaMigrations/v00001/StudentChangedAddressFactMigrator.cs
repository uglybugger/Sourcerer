using System;
using System.Collections.Generic;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure.Migrations;
using Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.SchemaMigrations.v00001
{
    public class StudentChangedAddressFactMigrator : IMigrateFact<StudentChangedAddressFact>
    {
        public IEnumerable<IFact> Migrate(IFact fact)
        {
            return Migrate((dynamic) fact);
        }

        public IEnumerable<IFact> Migrate(StudentChangedAddressFact fact)
        {
            var addressId = Guid.NewGuid();

            var addressCreatedFact = new AddressCreatedFact
                                     {
                                         AggregateRootId = addressId,
                                         StreetAddress = fact.StreetAddress,
                                         Suburb = fact.Suburb,
                                         State = fact.State,
                                         PostCode = fact.PostCode,
                                     };
            yield return addressCreatedFact;

            yield return new StudentAggregate.Facts.StudentChangedAddressFact
                         {
                             AggregateRootId = fact.AggregateRootId,
                             AddressId = addressId,
                         };
        }
    }
}