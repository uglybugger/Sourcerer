using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        public T Clone<T>() where T : class, IAggregateRoot
        {
            // FIXME the BinaryFormatter is *staggeringly* slow. It'll do for now but we need a replacement for this very soon.  -andrewh 9/3/2014
            var serializer = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, this);
                ms.Position = 0;
                var clone = (T)serializer.Deserialize(ms);
                return clone;
            }
        }
    }
}