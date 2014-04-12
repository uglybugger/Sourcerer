using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.Infrastructure;
using ThirdDrawer.Extensions.CollectionExtensionMethods;

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

        public void Add(T item)
        {
            _items.Value.TryAdd(item.Id, item);
        }

        public void Remove(T item)
        {
            T dummy;
            _items.Value.TryRemove(item.Id, out dummy);
        }

        public void Revert(IEnumerable<Guid> newItems, IEnumerable<Guid> modifiedItems, IEnumerable<Guid> removedItems)

        {
            lock (this)
            {
                T dummy;
                newItems.AsParallel()
                        .Do(id => _items.Value.TryRemove(id, out dummy))
                        .Done();

                removedItems.Union(modifiedItems)
                            .AsParallel()
                            .Select(id => _aggregateRebuilder.Rebuild<T>(id))
                            .Do(item => _items.Value[item.Id] = item)
                            .Do(item => _lastSeenRevisionIds[item.Id] = item.RevisionId)
                            .Done();
            }
        }
    }
}