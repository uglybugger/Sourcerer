using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure;

namespace Sourcerer.Persistence.Memory
{
    public class MemoryFactStore : IFactStore
    {
        private readonly ConcurrentDictionary<Guid, SortedList<UnitOfWorkProperties, IFact>> _streams = new ConcurrentDictionary<Guid, SortedList<UnitOfWorkProperties, IFact>>();
        private readonly object _mutex = new object();

        public void AppendAtomically(IFact[] facts)
        {
            lock (_mutex)
            {
                var aggregateRoots = facts.GroupBy(f => f.AggregateRootId);
                foreach (var grouping in aggregateRoots)
                {
                    var stream = _streams.GetOrAdd(grouping.Key, x => new SortedList<UnitOfWorkProperties, IFact>());
                    foreach (var fact in grouping)
                    {
                        stream.Add(fact.UnitOfWorkProperties, fact);
                    }
                }
            }
        }

        public IEnumerable<FactAbout<T>> GetStream<T>(Guid id) where T : IAggregateRoot
        {
            var stream = _streams.GetOrAdd(id, x => new SortedList<UnitOfWorkProperties, IFact>());

            lock (_mutex)
            {
                var facts = stream
                    .Values
                    .Cast<FactAbout<T>>()
                    .ToArray();
                return facts;
            }
        }

        public IEnumerable<Guid> GetAllStreamIds<T>() where T : IAggregateRoot
        {
            lock (_mutex)
            {
                return _streams.Keys.ToArray();
            }
        }
    }
}