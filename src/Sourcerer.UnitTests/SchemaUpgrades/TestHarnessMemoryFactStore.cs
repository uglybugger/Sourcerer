using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Persistence.Disk;
using Sourcerer.Persistence.Memory;

namespace Sourcerer.UnitTests.SchemaUpgrades
{
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