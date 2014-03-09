using System;
using System.Collections.Generic;
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
using Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate;

namespace Sourcerer.UnitTests.SchemaUpgrades
{
    [TestFixture]
    public class WhenUpgradingFromVersion0ToVersion2 : TestFor<SchemaMigrator>
    {
        private Guid _fredId;
        private Guid _wilmaId;
        private SchemaMigrator _schemaMigrator;
        private TestHarnessMemoryFactStore _factStoreV0;
        private TestHarnessMemoryFactStore _factStoreV1;
        private TestHarnessMemoryFactStore _factStoreV2;
        private IFactStoreFactory _factStoreFactory;
        private IDomainEventBroker _domainEventBroker;
        private SystemClock _systemClock;
        private AggregateRebuilder _aggregateRebuilderV2;
        private Snapshot<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student> _studentSnapshotV2;
        private Snapshot<Address> _addressSnapshotV2;

        protected override void Given()
        {
            var factAssembliesV0 = new[]
                                   {
                                       typeof (Student).Assembly
                                   };

            var factAssembliesV1 = new[]
                                   {
                                       typeof (SchemaUpgradeTests.v1.Domain.StudentAggregate.Student).Assembly
                                   };
            var factAssembliesV2 = new[]
                                   {
                                       typeof (SchemaUpgradeTests.v2.Domain.StudentAggregate.Student).Assembly
                                   };

            _domainEventBroker = Substitute.For<IDomainEventBroker>();
            _systemClock = new SystemClock();

            var typesProviderV0 = new AssemblyScanningTypesProvider(factAssembliesV0);
            var serializerV0 = new CustomXmlSerializer(typesProviderV0);

            var typesProviderV1 = new AssemblyScanningTypesProvider(factAssembliesV1);
            var serializerV1 = new CustomXmlSerializer(typesProviderV1);

            var typesProviderV2 = new AssemblyScanningTypesProvider(factAssembliesV2);
            var serializerV2 = new CustomXmlSerializer(typesProviderV2);

            _factStoreV0 = new TestHarnessMemoryFactStore(serializerV0, serializerV2);
            _factStoreV1 = new TestHarnessMemoryFactStore(serializerV2, serializerV2);
            _factStoreV2 = new TestHarnessMemoryFactStore(serializerV2, serializerV2);

            _factStoreFactory = Substitute.For<IFactStoreFactory>();
            _factStoreFactory.GetFactStore(0).Returns(_factStoreV0);
            _factStoreFactory.GetFactStore(1).Returns(_factStoreV1);
            _factStoreFactory.GetFactStore(2).Returns(_factStoreV2);

            var aggregateRebuilderV0 = new AggregateRebuilder(_factStoreV0);
            var studentSnapshotV0 = new Snapshot<Student>(aggregateRebuilderV0);

            // Create student in version 0 of schema
            using (var unitOfWork = new UnitOfWork(_factStoreV0, _domainEventBroker, _systemClock))
            {
                var repository = new Repository<Student>(studentSnapshotV0, unitOfWork);

                var fred = Student.Create("Fred", "Flintstone");

                // Make a change that will be refactored out in version 2
                fred.ChangeAddress("123 Imaginary St", "Bedrock", "BR", "65000000");

                _fredId = fred.Id;
                repository.Add(fred);

                unitOfWork.Complete();
            }

            using (var unitOfWork = new UnitOfWork(_factStoreV0, _domainEventBroker, _systemClock))
            {
                var repository = new Repository<Student>(studentSnapshotV0, unitOfWork);

                var wilma = Student.Create("Wilma", "Flintstone");

                // Make a change that will be refactored out in version 2
                wilma.ChangeAddress("123 Imaginary St", "Bedrock", "BR", "65000000");

                _wilmaId = wilma.Id;
                repository.Add(wilma);

                unitOfWork.Complete();
            }

            _schemaMigrator = new SchemaMigrator(factAssembliesV2,
                                                 t => t.Namespace.Contains("SchemaMigrations"),
                                                 t => int.Parse(t.Namespace.Split('.').Last().TrimStart('v')),
                                                 _factStoreFactory
                );
            _aggregateRebuilderV2 = new AggregateRebuilder(_factStoreV2);
            _studentSnapshotV2 = new Snapshot<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(_aggregateRebuilderV2);
            _addressSnapshotV2 = new Snapshot<Address>(_aggregateRebuilderV2);
        }

        protected override void When()
        {
            _schemaMigrator.DoYourThing();
        }

        [Test]
        public void FredShouldExist()
        {
            using (var unitOfWork = new UnitOfWork(_factStoreV2, _domainEventBroker, _systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(_studentSnapshotV2, unitOfWork);
                var fred = studentRepository.GetById(_fredId);
                fred.ShouldNotBe(null);
            }
        }

        [Test]
        public void FredsGivenNameShouldBeCorrect()
        {
            using (var unitOfWork = new UnitOfWork(_factStoreV2, _domainEventBroker, _systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(_studentSnapshotV2, unitOfWork);
                var fred = studentRepository.GetById(_fredId);
                fred.GivenName.ShouldBe("Fred");
            }
        }

        [Test]
        public void FredsFamilyNameShouldBeCorrect()
        {
            using (var unitOfWork = new UnitOfWork(_factStoreV2, _domainEventBroker, _systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(_studentSnapshotV2, unitOfWork);
                var fred = studentRepository.GetById(_fredId);
                fred.FamilyName.ShouldBe("Flintstone");
            }
        }

        [Test]
        public void WilmaShouldExist()
        {
            using (var unitOfWork = new UnitOfWork(_factStoreV2, _domainEventBroker, _systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(_studentSnapshotV2, unitOfWork);
                var wilma = studentRepository.GetById(_wilmaId);
                wilma.ShouldNotBe(null);
            }
        }

        [Test]
        public void WilmasGivenNameShouldBeCorrect()
        {
            using (var unitOfWork = new UnitOfWork(_factStoreV2, _domainEventBroker, _systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(_studentSnapshotV2, unitOfWork);
                var wilma = studentRepository.GetById(_wilmaId);
                wilma.GivenName.ShouldBe("Wilma");
            }
        }

        [Test]
        public void WilmasFamilyNameShouldBeCorrect()
        {
            using (var unitOfWork = new UnitOfWork(_factStoreV2, _domainEventBroker, _systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(_studentSnapshotV2, unitOfWork);
                var wilma = studentRepository.GetById(_wilmaId);
                wilma.FamilyName.ShouldBe("Flintstone");
            }
        }

        [Test]
        public void FredsAddressShouldBeCorrect()
        {
            using (var unitOfWork = new UnitOfWork(_factStoreV2, _domainEventBroker, _systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(_studentSnapshotV2, unitOfWork);
                var addressRepository = new Repository<Address>(_addressSnapshotV2, unitOfWork);

                var fred = studentRepository.GetById(_fredId);
                var fredAddress = addressRepository.GetById(fred.AddressId);
                fredAddress.StreetAddress.ShouldBe("123 Imaginary St");
            }
        }

        [Test]
        public void FredAndWilmaShouldLiveAtTheSameAddressInstance()
        {
            using (var unitOfWork = new UnitOfWork(_factStoreV2, _domainEventBroker, _systemClock))
            {
                var studentRepository = new Repository<SchemaUpgradeTests.v2.Domain.StudentAggregate.Student>(_studentSnapshotV2, unitOfWork);
                var addressRepository = new Repository<Address>(_addressSnapshotV2, unitOfWork);

                var fred = studentRepository.GetById(_fredId);
                var wilma = studentRepository.GetById(_wilmaId);

                var fredAddress = addressRepository.GetById(fred.AddressId);
                var wilmaAddress = addressRepository.GetById(wilma.AddressId);

                fredAddress.Id.ShouldBe(wilmaAddress.Id);
            }
        }
    }

    [TestFixture]
    public abstract class TestFor<T>
    {
        protected abstract void Given();
        protected abstract void When();

        [SetUp]
        public void SetUp()
        {
            Given();
            When();
        }
    }

    internal class TestHarnessMemoryFactStore : MemoryFactStore
    {
        private readonly CustomXmlSerializer _serializer;
        private readonly CustomXmlSerializer _deserializer;

        public TestHarnessMemoryFactStore(CustomXmlSerializer serializer, CustomXmlSerializer deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        public override IEnumerable<IGrouping<Guid, IFact>> GetAllFactsGroupedByUnitOfWork()
        {
            return base.GetAllFactsGroupedByUnitOfWork()
                       .SelectMany(uow => uow)
                       .Select(fact => _serializer.Serialize(fact))
                       .Select(xml => _deserializer.Deserialize<IFact>(xml))
                       .GroupBy(fact => fact.UnitOfWorkProperties.UnitOfWorkId);
        }
    }
}