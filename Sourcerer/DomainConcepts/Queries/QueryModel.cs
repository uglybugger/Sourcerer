using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.Infrastructure;

namespace Sourcerer.DomainConcepts.Queries
{
    public class QueryModel<T> : IQueryModel<T> where T : class, IAggregateRoot
    {
        private readonly IAggregateRebuilder _aggregateRebuilder;
        private readonly Lazy<ConcurrentDictionary<Guid, T>> _items;

        private readonly ConcurrentDictionary<Guid, Guid> _lastSeenRevisionIds = new ConcurrentDictionary<Guid, Guid>();

        public QueryModel(IAggregateRebuilder aggregateRebuilder)
        {
            _aggregateRebuilder = aggregateRebuilder;
            _items = new Lazy<ConcurrentDictionary<Guid, T>>(RebuildAll);
        }

        private ConcurrentDictionary<Guid, T> RebuildAll()
        {
            return new ConcurrentDictionary<Guid, T>(_aggregateRebuilder.RebuildAll<T>().ToDictionary(a => a.Id, a => a));
        }

        public T GetById(Guid itemId)
        {
            return _items.Value[itemId];
        }

        public IQueryable<T> Items
        {
            get { return _items.Value.Values.AsQueryable(); }
        }

        public void UpdateAtomically(IEnumerable<T> newItems, IEnumerable<T> modifiedItems, IEnumerable<T> removedItems)

        {
            lock (this)
            {
                foreach (var item in newItems)
                {
                    _items.Value[item.Id] = item;
                    _lastSeenRevisionIds[item.Id] = item.RevisionId;
                }

                foreach (var item in removedItems)
                {
                    T dummy;
                    _items.Value.TryRemove(item.Id, out dummy);
                }

                foreach (var item in modifiedItems)
                {
                    T existing;
                    _items.Value.TryGetValue(item.Id, out existing);

                    var replacement = NeedsRebuild(existing, item)
                        ? _aggregateRebuilder.Rebuild<T>(item.Id)
                        : item;

                    _lastSeenRevisionIds[item.Id] = item.RevisionId;
                    _items.Value[replacement.Id] = replacement;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool NeedsRebuild(IAggregateRoot existing, IAggregateRoot updated)
        {
            if (existing == null) return false;
            if (existing.RevisionId == Guid.Empty) return false;

            var lastSeenRevisionId = _lastSeenRevisionIds[updated.Id];
            if (existing.RevisionId == lastSeenRevisionId) return false;

            return true;
        }
    }
}