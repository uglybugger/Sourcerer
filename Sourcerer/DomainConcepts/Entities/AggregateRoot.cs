using System;
using System.Collections.Generic;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.DomainConcepts.Entities
{
    [Serializable]
    public abstract class AggregateRoot : Entity, IAggregateRoot
    {
        private readonly List<IFact> _pendingFacts = new List<IFact>();

        protected internal override void Append(IFact fact)
        {
            lock (_pendingFacts)
            {
                _pendingFacts.Add(fact);
            }
        }

        public IEnumerable<IFact> GetAndClearPendingFacts()
        {
            lock (_pendingFacts)
            {
                var facts = _pendingFacts.ToArray();
                _pendingFacts.Clear();
                return facts;
            }
        }

        public Guid RevisionId { get; set; }
    }
}