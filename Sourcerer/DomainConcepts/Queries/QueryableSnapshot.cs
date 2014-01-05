using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure;
using Sourcerer.Persistence;

namespace Sourcerer.DomainConcepts.Queries
{
    public class QueryableSnapshot : IQueryableSnapshot
    {
        private readonly IFactStore _factStore;
        private readonly IAggregateRebuilder _aggregateRebuilder;

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, IAggregateRoot>> _items =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, IAggregateRoot>>();

        public QueryableSnapshot(IFactStore factStore, IAggregateRebuilder aggregateRebuilder)
        {
            _factStore = factStore;
            _aggregateRebuilder = aggregateRebuilder;
        }

        public T GetById<T>(Guid id) where T : IAggregateRoot
        {
            var items = GetItemDictionary(typeof (T));
            return (T) items[id];
        }

        public T TryGetById<T>(Guid id) where T : IAggregateRoot
        {
            var items = GetItemDictionary(typeof (T));
            IAggregateRoot result;
            return (T) (items.TryGetValue(id, out result) ? result : default(T));
        }

        IQueryable<T> IQueryableSnapshot.Items<T>()
        {
            return _items.GetOrAdd(typeof (T), GetItemDictionary)
                         .Values
                         .Cast<T>()
                         .AsQueryable();
        }

        public void NotifyFactsWereCommitted(IEnumerable<IFact> facts)
        {
            foreach (var fact in facts)
            {
                var entityType = Type.GetType(fact.EntityTypeName);
                var items = GetItemDictionary(entityType);
                var openGenericMethod = typeof (IAggregateRebuilder).GetMethod("Rebuild");
                var closedGenericMethod = openGenericMethod.MakeGenericMethod(entityType);

                lock (items)
                {
                    var item = (IAggregateRoot) closedGenericMethod.Invoke(_aggregateRebuilder, new object[] {fact.AggregateRootId}); //FIXME make compile-time safe somehow
                    items[item.Id] = item;
                }
            }
        }

        private ConcurrentDictionary<Guid, IAggregateRoot> GetItemDictionary(Type type)
        {
            return _items.GetOrAdd(type, ConstructItemDictionary);
        }

        private ConcurrentDictionary<Guid, IAggregateRoot> ConstructItemDictionary(Type type)
        {
            var openGenericMethod = typeof (QueryableSnapshot).GetMethod("LoadAll", BindingFlags.Instance | BindingFlags.NonPublic);
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(type);
            var result = (ConcurrentDictionary<Guid, IAggregateRoot>) closedGenericMethod.Invoke(this, null);
            return result;
        }

        // ReSharper disable UnusedMember.Local
        // Invoked via reflection
        private ConcurrentDictionary<Guid, IAggregateRoot> LoadAll<T>() where T : IAggregateRoot
            // ReSharper restore UnusedMember.Local
        {
            var entities = _factStore
                .GetAllStreamIds<T>()
                .Select(id => new KeyValuePair<Guid, IAggregateRoot>(id, _aggregateRebuilder.Rebuild<T>(id)))
                .ToArray()
                ;

            return new ConcurrentDictionary<Guid, IAggregateRoot>(entities);
        }
    }
}