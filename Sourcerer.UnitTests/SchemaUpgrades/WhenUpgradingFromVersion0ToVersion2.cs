using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Migrations;
using Sourcerer.Infrastructure.Time;
using Sourcerer.Persistence.Disk;
using Sourcerer.Persistence.Memory;
using Sourcerer.SchemaUpgradeTests.v0.Domain.StudentAggregate;
using Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate;

namespace Sourcerer.UnitTests.SchemaUpgrades
{
    [TestFixture]
    public class WhenUpgradingFromVersion0ToVersion2
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

            // Upgrade schema to version 2 via the SchemaMigrator

            var factAssembliesV3 = new[]
                                   {
                                       typeof (SchemaUpgradeTests.v2.Domain.StudentAggregate.Student).Assembly
                                   };

#error need to use a TestHarnessMemoryFactStore that will serialize/deserialize to/from XML when we extract all the facts so that we can map between versions easily.
            var factStoreFactory = Substitute.For<IFactStoreFactory>();
            factStoreFactory.GetFactStore(0).Returns(factStoreV0);
            var factStoreV1 = new MemoryFactStore();
            factStoreFactory.GetFactStore(1).Returns(factStoreV1);
            var factStoreV2 = new MemoryFactStore();
            factStoreFactory.GetFactStore(2).Returns(factStoreV2);

            var schemaMigrator = new SchemaMigrator(factAssembliesV3,
                                                    t => t.Namespace.Contains("SchemaMigrations"),
                                                    t => int.Parse(t.Namespace.Split('.').Last().TrimStart('v')),
                                                    factStoreFactory
                );
            schemaMigrator.DoYourThing();

            var aggregateRebuilderV2 = new AggregateRebuilder(factStoreV2);
            var queryableSnapshotV2 = new QueryableSnapshot(factStoreV2, aggregateRebuilderV2);

            // Assert that our changes have come across correctly
            using (var unitOfWork = new UnitOfWork(factStoreV2, eventBroker, queryableSnapshotV2, systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(unitOfWork, queryableSnapshotV2);
                var addressRepository = new Repository<Address>(unitOfWork, queryableSnapshotV2);

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