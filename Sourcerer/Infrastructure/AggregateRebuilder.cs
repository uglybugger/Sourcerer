using System;
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

        public T Rebuild<T>(Guid id) where T : IAggregateRoot
        {
            var facts = _factStore.GetStream<T>(id);
            var aggregateRoot = (T) Activator.CreateInstance(typeof (T), true);
            foreach (var fact in facts)
            {
                ((dynamic) aggregateRoot).Apply((dynamic) fact);
            }

            return aggregateRoot;
        }
    }
}