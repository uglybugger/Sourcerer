using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure;

namespace Sourcerer.DomainConcepts.Queries
{
    public class QueryableSnapshot : IQueryableSnapshot
    {
        private readonly IFactStore _factStore;
        private readonly IAggregateRebuilder _aggregateRebuilder;

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, IAggregateRoot>> _items =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, IAggregateRoot>>();

        private readonly ConcurrentDictionary<Guid, Guid> _lastSeenRevisionIds = new ConcurrentDictionary<Guid, Guid>();

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

        public void NotifyAggregateRootsModified(IList<IAggregateRoot> aggregateRoots, IList<IFact> facts)
        {
            var updatedIds = facts.Select(f => f.AggregateRootId).Distinct().ToArray();

            foreach (var updatedId in updatedIds)
            {
                var updated = aggregateRoots.Where(ar => ar.Id == updatedId).First();
                var entityType = updated.GetType();
                var items = GetItemDictionary(entityType);

                IAggregateRoot existing;
                IAggregateRoot replacement;

                items.TryGetValue(updated.Id, out existing);
                if (NeedsRebuild(existing, updated))
                {
                    var openGenericMethod = typeof (IAggregateRebuilder).GetMethod("Rebuild");
                    var closedGenericMethod = openGenericMethod.MakeGenericMethod(entityType);
                    replacement = (IAggregateRoot) closedGenericMethod.Invoke(_aggregateRebuilder, new object[] {updated.Id}); //FIXME make compile-time safe somehow
                }
                else
                {
                    replacement = updated;
                }

                lock (items)
                {
                    _lastSeenRevisionIds[updated.Id] = updated.RevisionId;
                    items[replacement.Id] = replacement;
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