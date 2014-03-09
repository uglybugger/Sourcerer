using System;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;

namespace Sourcerer.Infrastructure
{
    public class AggregateRebuilder : IAggregateRebuilder
    {
        private readonly IFactStore _factStore;

        public AggregateRebuilder(IFactStore factStore)
        {
            _factStore = factStore;
        }

        public T Rebuild<T>(Guid id) where T : class, IAggregateRoot
        {
            var facts = _factStore.GetStream<T>(id);
            var aggregateRoot = (T) Activator.CreateInstance(typeof (T), true);
            foreach (var fact in facts)
            {
                ((dynamic) aggregateRoot).Apply((dynamic) fact);
            }

            return aggregateRoot;
        }

        public T[] RebuildAll<T>() where T : class, IAggregateRoot
        {
            var streamIds = _factStore.GetAllStreamIds<T>();
            var aggregateRoots = streamIds
                .Select(id => Rebuild<T>(id))
                .ToArray();
            return aggregateRoots;
        }
    }
}