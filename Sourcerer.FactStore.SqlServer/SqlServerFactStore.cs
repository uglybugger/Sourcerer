using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure;
using Sourcerer.Persistence.Disk;
using ThirdDrawer.Extensions.CollectionExtensionMethods;

namespace Sourcerer.FactStore.SqlServer
{
    public class SqlServerFactStore : IFactStore
    {
        private readonly Func<IFactStoreDataContext> _contextFactory;
        private readonly ICustomSerializer _serializer;

        public static SqlServerFactStore Create(string connectionString, ITypesProvider typesProvider)
        {
            return new SqlServerFactStore(() => new FactStoreDataContext(connectionString), new CustomXmlSerializer(typesProvider));
        }

        internal SqlServerFactStore(Func<IFactStoreDataContext> contextFactory, ICustomSerializer serializer)
        {
            _contextFactory = contextFactory;
            _serializer = serializer;
        }

        public void AppendAtomically(IFact[] facts)
        {
            if (!facts.Any()) return;

            using (var context = _contextFactory())
            {
                var serializedFacts = facts
                    .AsParallel()
                    .Select(f => new SerializedFact
                                 {
                                     AggregateRootId = f.AggregateRootId,
                                     Timestamp = f.UnitOfWorkProperties.FactTimestamp,
                                     UnitOfWorkId = f.UnitOfWorkProperties.UnitOfWorkId,
                                     SequenceNumber = f.UnitOfWorkProperties.SequenceNumber,
                                     StreamTypeId = StreamTypeIdFor(f),
                                     FactBlob = _serializer.Serialize(f),
                                 })
                    .ToArray();
                context.Facts.AddRange(serializedFacts);

                context.SaveChanges();
            }
        }

        public IEnumerable<FactAbout<T>> GetStream<T>(Guid id) where T : IAggregateRoot
        {
            using (var context = _contextFactory())
            {
                var streamTypeId = StreamTypeIdFor(typeof (T));
                var facts = context.Facts
                                   .Where(f => f.AggregateRootId == id)
                                   .Where(f => f.StreamTypeId == streamTypeId)
                                   .Select(f => f.FactBlob)
                                   .AsEnumerable()
                                   .Select(blob => _serializer.Deserialize<FactAbout<T>>(blob))
                                   .ToArray();

                if (!facts.Any()) throw new Exception("No facts found!");

                return facts;
            }
        }

        public IEnumerable<Guid> GetAllStreamIds<T>() where T : IAggregateRoot
        {
            using (var context = _contextFactory())
            {
                var streamTypeId = StreamTypeIdFor(typeof (T));
                return context.Facts
                              .Where(f => f.StreamTypeId == streamTypeId)
                              .GroupBy(f => f.AggregateRootId)
                              .Select(g => g.Key)
                              .Distinct()
                              .ToArray();
            }
        }

        public IEnumerable<IGrouping<Guid, IFact>> GetAllFactsGroupedByUnitOfWork()
        {
            using (var context = _contextFactory())
            {
                return context.Facts
                              .AsEnumerable()
                              .Select(f => _serializer.Deserialize<FactAbout<IAggregateRoot>>(f.FactBlob))
                              .GroupBy(f => f.UnitOfWorkProperties.UnitOfWorkId);
            }
        }

        public void ImportFrom(IEnumerable<IFact> facts)
        {
            AppendAtomically(facts.ToArray());
        }

        private Guid StreamTypeIdFor(IFact fact)
        {
            return StreamTypeIdFor(fact.StreamName);
        }

        private Guid StreamTypeIdFor(Type aggregateRootType)
        {
            var streamName = aggregateRootType.Name;
            return StreamTypeIdFor(streamName);
        }

        private Guid StreamTypeIdFor(string streamName)
        {
            using (var context = _contextFactory())
            {
                var streamType = context.StreamType
                                        .Where(st => string.Compare(st.StreamName, streamName, StringComparison.Ordinal) == 0)
                                        .FirstOrDefault();
                if (streamType != null) return streamType.Id;

                streamType = new StreamType
                             {
                                 Id = Guid.NewGuid(),
                                 StreamName = streamName,
                             };

                context.StreamType.Add(streamType);
                context.SaveChanges();
                return streamType.Id;
            }
        }

        public bool HasFacts
        {
            get
            {
                using (var context = _contextFactory())
                {
                    return context.Facts.Any();
                }
            }
        }
    }
}