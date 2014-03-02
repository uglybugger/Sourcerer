using System;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Queries;
using Sourcerer.Infrastructure;
using Sourcerer.Infrastructure.Time;
using Sourcerer.Persistence.Memory;

namespace Sourcerer.UnitTests.SchemaUpgrades
{
    [TestFixture]
    public class WhenUpgradingFromVersion1ToVersion2
    {
        [Test]
        public void NothingShouldGoBang()
        {
            Guid fredId;
            Guid wilmaId;

            var eventBroker = Substitute.For<IDomainEventBroker>();
            var systemClock = new SystemClock();

            var factStoreV1 = new MemoryFactStore();
            var aggregateRebuilderV1 = new AggregateRebuilder(factStoreV1);
            var queryableSnapshotV1 = new QueryableSnapshot(factStoreV1, aggregateRebuilderV1);

            // Create student in version 1 of schema
            using (var unitOfWork = new UnitOfWork(factStoreV1, eventBroker, queryableSnapshotV1, systemClock))
            {
                var repository = new Repository<Sourcerer.SchemaUpgradeTests.v1.Domain.StudentAggregate.Student>(unitOfWork, queryableSnapshotV1);

                var fred = SchemaUpgradeTests.v1.Domain.StudentAggregate.Student.Create("Fred", "Flintstone");

                // Make a change that will be refactored out in version 2
                fred.ChangeAddress("123 Imaginary St", "Bedrock", "BR", "65000000");

                fredId = fred.Id;
                repository.Add(fred);

                unitOfWork.Commit();
            } 
            
            using (var unitOfWork = new UnitOfWork(factStoreV1, eventBroker, queryableSnapshotV1, systemClock))
            {
                var repository = new Repository<Sourcerer.SchemaUpgradeTests.v1.Domain.StudentAggregate.Student>(unitOfWork, queryableSnapshotV1);

                var wilma = SchemaUpgradeTests.v1.Domain.StudentAggregate.Student.Create("Wilma", "Flintstone");

                // Make a change that will be refactored out in version 2
                wilma.ChangeAddress("123 Imaginary St", "Bedrock", "BR", "65000000");

                wilmaId = wilma.Id;
                repository.Add(wilma);

                unitOfWork.Commit();
            } 
            
          
            // Upgrade schema to version 2
            var factStoreV2 = new MemoryFactStore();
            var aggregateRebuilderV2 = new AggregateRebuilder(factStoreV2);
            var queryableSnapshotV2 = new QueryableSnapshot(factStoreV2, aggregateRebuilderV2);

            // Assert that our changes have come across correctly
            using (var unitOfWork = new UnitOfWork(factStoreV2, eventBroker, queryableSnapshotV2, systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(unitOfWork, queryableSnapshotV2);
                var addressRepository = new Repository<SchemaUpgradeTests.v2.Domain.AddressAggregate.Address>(unitOfWork, queryableSnapshotV2);

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
