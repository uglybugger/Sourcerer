using System.Collections.Generic;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure.Migrations;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.SchemaMigrations.v00002
{
    public class StudentCreatedFactMigrator : IMigrateFact<StudentCreatedFact>
    {
        public IEnumerable<IFact> Migrate(IFact fact, MigrationContext context)
        {
            return Migrate((dynamic) fact, context);
        }

        public IEnumerable<IFact> Migrate(StudentCreatedFact fact, MigrationContext context)
        {
            yield return new StudentAggregate.Facts.StudentCreatedFact
                         {
                             AggregateRootId = fact.AggregateRootId,
                             GivenName = fact.FirstName,
                             FamilyName = fact.LastName,
                         };
        }
    }
}