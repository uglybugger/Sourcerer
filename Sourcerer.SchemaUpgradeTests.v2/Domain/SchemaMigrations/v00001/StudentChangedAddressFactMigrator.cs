using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure.Migrations;
using Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.SchemaMigrations.v00001
{
    public class StudentChangedAddressFactMigrator : IMigrateFact<StudentChangedAddressFact>
    {
        public IEnumerable<IFact> Migrate(IFact fact, MigrationContext context)
        {
            return Migrate((dynamic) fact, context);
        }

        public IEnumerable<IFact> Migrate(StudentChangedAddressFact fact, MigrationContext context)
        {
            // have we created an address AR for this before?
            var priorAddressCreatedFact = context.Facts
                                                 .OfType<AddressCreatedFact>()
                                                 .Where(f => f.StreetAddress == fact.StreetAddress)
                                                 .Where(f => f.Suburb == fact.Suburb)
                                                 .Where(f => f.State == fact.State)
                                                 .Where(f => f.PostCode == fact.PostCode)
                                                 .SingleOrDefault();

            Guid addressId;
            if (priorAddressCreatedFact != null)
            {
                addressId = priorAddressCreatedFact.AggregateRootId;
            }
            else
            {
                addressId = Guid.NewGuid();

                var addressCreatedFact = new AddressCreatedFact
                                         {
                                             AggregateRootId = addressId,
                                             StreetAddress = fact.StreetAddress,
                                             Suburb = fact.Suburb,
                                             State = fact.State,
                                             PostCode = fact.PostCode,
                                         };
                yield return addressCreatedFact;
            }

            yield return new StudentAggregate.Facts.StudentChangedAddressFact
                         {
                             AggregateRootId = fact.AggregateRootId,
                             AddressId = addressId,
                         };
        }
    }
}