using System;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.DomainConcepts.Entities
{
    [Serializable]
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        protected internal abstract void Append(IFact fact);
    }
}