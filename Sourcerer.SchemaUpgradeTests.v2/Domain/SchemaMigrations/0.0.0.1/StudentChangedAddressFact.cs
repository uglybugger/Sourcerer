using System.Collections;
using System.Collections.Generic;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate;
using Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.SchemaMigrations
{
    //Sourcerer.SchemaUpgradeTests.v1.Domain.StudentAggregate.Facts
    public class StudentChangedAddressFact : FactAbout<Student>
    {
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
    }

    public class StudentChangedAddressFactMigrator: IMigrateFact<StudentChangedAddressFact>
    {
        public IEnumerable<IFact> Migrate(StudentChangedAddressFact fact)
        {
            var address = Address.Create(fact.StreetAddress, fact.Suburb, fact.State, fact.PostCode);
            foreach (var addressFacts in address.GetAndClearPendingFacts()) yield return addressFacts;

            yield return new StudentAggregate.Facts.StudentChangedAddressFact
            {
                AggregateRootId = fact.AggregateRootId,
                AddressId = address.Id,
            };
        }
    }

    public interface IMigrateFact<TFact>
    {
        IEnumerable<IFact> Migrate(TFact fact); 
    }
}