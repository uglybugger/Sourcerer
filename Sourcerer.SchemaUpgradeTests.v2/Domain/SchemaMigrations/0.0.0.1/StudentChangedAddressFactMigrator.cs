using System.Collections.Generic;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure.Migrations;
using Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.SchemaMigrations
{
    public class StudentChangedAddressFactMigrator : IMigrateFact<StudentChangedAddressFact>
    {
        public IEnumerable<IFact> Migrate(IFact fact)
        {
            return Migrate((dynamic) fact);
        }

        public IEnumerable<IFact> Migrate(StudentChangedAddressFact fact)
        {
            var address = Address.Create(fact.StreetAddress, fact.Suburb, fact.State, fact.PostCode);
            foreach (var addressFact in address.GetAndClearPendingFacts()) yield return addressFact;

            yield return new StudentAggregate.Facts.StudentChangedAddressFact
                         {
                             AggregateRootId = fact.AggregateRootId,
                             AddressId = address.Id,
                         };
        }
    }
}