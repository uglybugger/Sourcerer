using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Migrations;
using Sourcerer.Infrastructure.Time;
using Sourcerer.Persistence.Disk;
using Sourcerer.Persistence.Memory;
using Sourcerer.SchemaUpgradeTests.v0.Domain.StudentAggregate;
using Sourcerer.SchemaUpgradeTests.v1.Domain.AddressAggregate;
using Sourcerer.SchemaUpgradeTests.v1.Domain.SchemaMigrations.v00001;

namespace Sourcerer.UnitTests.SchemaUpgrades
{
    [TestFixture]
    public class WhenUpgradingFromVersion0ToVersion1
    {
        [Test]
        public void NothingShouldGoBang()
        {
            var factAssembliesV0 = new[]
                                   {
                                       typeof (Student).Assembly
                                   };

            Guid fredId;
            Guid wilmaId;

            var eventBroker = Substitute.For<IDomainEventBroker>();
            var systemClock = new SystemClock();

            var typesProviderV0 = new AssemblyScanningTypesProvider(factAssembliesV0);
            var serializerV0 = new CustomXmlSerializer(typesProviderV0);
            var factStoreV0 = new MemoryFactStore();
            var aggregateRebuilderV0 = new AggregateRebuilder(factStoreV0);
            var queryableSnapshotV0 = new QueryableSnapshot(factStoreV0, aggregateRebuilderV0);

            // Create student in version 0 of schema
            using (var unitOfWork = new UnitOfWork(factStoreV0, eventBroker, queryableSnapshotV0, systemClock))
            {
                var repository = new Repository<Student>(unitOfWork, queryableSnapshotV0);

                var fred = Student.Create("Fred", "Flintstone");

                // Make a change that will be refactored out in version 2
                fred.ChangeAddress("123 Imaginary St", "Bedrock", "BR", "65000000");

                fredId = fred.Id;
                repository.Add(fred);

                unitOfWork.Commit();
            }

            using (var unitOfWork = new UnitOfWork(factStoreV0, eventBroker, queryableSnapshotV0, systemClock))
            {
                var repository = new Repository<Student>(unitOfWork, queryableSnapshotV0);

                var wilma = Student.Create("Wilma", "Flintstone");

                // Make a change that will be refactored out in version 2
                wilma.ChangeAddress("123 Imaginary St", "Bedrock", "BR", "65000000");

                wilmaId = wilma.Id;
                repository.Add(wilma);

                unitOfWork.Commit();
            }

            // Upgrade schema to version 1

            var factAssembliesV1 = new[]
                                   {
                                       typeof (SchemaUpgradeTests.v2.Domain.StudentAggregate.Student).Assembly
                                   };
            var typesProviderV1 = new AssemblyScanningTypesProvider(factAssembliesV1);
            var serializerV1 = new CustomXmlSerializer(typesProviderV1);

            // we don't do this for realsies - we're simulating two different appdomains' having
            // persisted and then rehydrated our fact objects.
            var factsV1 = factStoreV0.GetAllFactsGroupedByUnitOfWork()
                                     .SelectMany(uow => uow)
                                     .Select(serializerV0.Serialize)
                                     .Select(serializerV1.Deserialize<IFact>)
                                     .GroupBy(f => f.UnitOfWorkProperties.UnitOfWorkId)
                                     .ToArray();

            var factStoreV1 = new MemoryFactStore();

            var migratorTypes = new[] {typeof (StudentChangedAddressFactMigrator)};

            var migrator = new VersionMigrator(migratorTypes);
            var migratedFacts = migrator.Migrate(factsV1);
            factStoreV1.ImportFrom(migratedFacts);

            var aggregateRebuilderV1 = new AggregateRebuilder(factStoreV1);
            var queryableSnapshotV1 = new QueryableSnapshot(factStoreV1, aggregateRebuilderV1);

            // Assert that our changes have come across correctly
            using (var unitOfWork = new UnitOfWork(factStoreV1, eventBroker, queryableSnapshotV1, systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(unitOfWork, queryableSnapshotV1);
                var addressRepository = new Repository<Address>(unitOfWork, queryableSnapshotV1);

                var fred = studentRepository.GetById(fredId);
                var wilma = studentRepository.GetById(wilmaId);

                var fredAddress = addressRepository.GetById(fred.AddressId);
                fredAddress.StreetAddress.ShouldBe("123 Imaginary St");

                var wilmaAddress = addressRepository.GetById(wilma.AddressId);
                wilmaAddress.StreetAddress.ShouldBe("123 Imaginary St");

                fredAddress.Id.ShouldBe(wilmaAddress.Id);
            }
        }
    }
}